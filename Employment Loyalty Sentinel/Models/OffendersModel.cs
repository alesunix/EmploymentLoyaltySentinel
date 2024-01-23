using EmploymentLoyaltySentinel.Extensions;
using EmploymentLoyaltySentinel.Shared;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EmploymentLoyaltySentinel.Models
{
    public class OffendersModel : BaseModel
    {
        public bool IsMoney { get; set; }
        public decimal Id { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Employee { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Dol { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Grpprg { get; set; }
        public string Grpprgname { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Region { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Offense { get; set; }
        public DateTime Dr { get; set; }
        public DateTime Dp { get; set; }
        public decimal Summ { get; set; }
        public decimal Balance { get; set; }
        [RequiredIf(nameof(IsMoney), true, ErrorMessage = "Поле не должно быть пустым!")]
        public string Cur { get; set; }
        public string Curname { get; set; }
        public decimal Loginid { get; set; }
        public decimal Loginid_lastedit { get; set; }
        public DateTime Date_record { get; set; }
        public DateTime Date_lastedit { get; set; }
        public string Status { get; set; }
        public string Auditor { get; set; }
        public object Photo { get; set; }
        public string Deleted { get; set; }
        public string Inbase { get; set; }
        public decimal FileID { get; set; }
        public decimal OffenseID { get; set; }
        public MainLayout Layout { get; set; }
        public List<OffendersModel> GetDataOffenders(bool isDeleted, bool isOperator, string filter = null, decimal id = 0)
        {
            char del = isDeleted == true ? 'T' : 'F';
            filter ??= string.Empty;
            string idWhere = string.Empty;
            string grpprgWhere = string.Empty;
            string onlyCheked = string.Empty;

            switch (isOperator)
            {
                case true:
                    grpprgWhere = $" AND a.grpprg = '{GetGrpprg()}'";
                    break;
            }
            if (id != 0)
                idWhere = $" AND a.id = {id}";

            if (!string.IsNullOrWhiteSpace(filter))
                onlyCheked = " AND a.status <> 'U'";

            var list = GetTList<OffendersModel>($@"SELECT a.id, a.employee, a.dol, a.dr, a.dp, a.grpprg, a.summ, a.cur,
                                                a.loginid, a.loginid_lastedit, a.date_record, a.date_lastedit,
                                                a.status, a.auditor, a.deleted, a.photo, a.region, a.balance, a.inbase, b.nam as grpprgname, c.nam as curname
                                                FROM offenders a
                                                left join sprgrpprg_v b on a.grpprg = b.grpprg 
                                                left join sprcur_v c on a.cur = c.cur
                                                WHERE deleted = '{del}'{grpprgWhere} {idWhere} {onlyCheked} AND LOWER(a.Employee) || ' ' LIKE '%{filter.ToLower()}%' 
                                                ORDER BY status DESC, date_record DESC");
            return list;
        }
        public string GetGrpprg() => GetSingleResult($"SELECT grpprg FROM userid WHERE id = {Loginid}").ToString();
        public string GetRegion() => GetSingleResult($@"SELECT b.key as region
                                                          FROM userid a
                                                          left join sprregion_v b on a.region = b.key WHERE a.id = {Loginid}").ToString();
        public string GetEmployeeForUserId(decimal id) => GetSingleResult($"SELECT Employee FROM userid WHERE id = {id}").ToString();
        public Dictionary<string, string> YesNoList()
        {
            Dictionary<string, string> list = new();
            list.Add("U", "Не определено");
            list.Add("T", "Разрешается");
            list.Add("F", "Запрещается");
            return list;
        }
        public List<Employee> GetListEmployee(string filter, bool IsRoleAdminOrAuditor)
        {
            filter ??= string.Empty;
            string where = string.Empty;
            switch (IsRoleAdminOrAuditor)
            {
                case false:
                    where = $"b.id = {Loginid} AND ";
                    break;
                case true:
                    where = $"rownum <= 200 AND ";
                    break;
            }
            return GetTList<Employee>($@"SELECT a.fio, a.otv, a.dol, a.lev as region, a.grpprg, a.dr, a.de
                                            FROM sprotv_mv a
                                            left join userid b on a.grpprg = b.grpprg
                                            WHERE {where} LOWER(a.fio) LIKE '%{filter.ToLower()}%' ORDER BY a.fio");
        }
        public List<string> GetListDol() => GetList("SELECT distinct dol FROM sprotv_mv");
        public Dictionary<string, string> GetListRegion() => GetDictionary<string, string>($"SELECT key, nam FROM sprregion_v ");
        public Dictionary<string, string> GetListGrpprg(string filter = null, string con = "T")
        {
            filter ??= string.Empty;
            return GetDictionary<string, string>($"SELECT grpprg, grpprg || ' - ' || nam as nam FROM sprgrpprg_v WHERE con = '{con}' AND LOWER(nam) LIKE '%{filter.ToLower()}%' ORDER BY nam");
        }
        public Dictionary<decimal, string> GetListOffense(string filter = null)
        {
            filter ??= string.Empty;
            return GetDictionary<decimal, string>($"SELECT id, nam FROM sproffense WHERE deleted = 'F' AND LOWER(nam) LIKE '%{filter.ToLower()}%' ORDER BY nam");
        }
        public Dictionary<string, string> GetListCur() => GetDictionary<string, string>("SELECT cur, nam FROM sprcur_v ORDER BY nam DESC");
        public Dictionary<decimal, string> GetOffenses(decimal id) => GetDictionary<decimal, string>($"SELECT id, offense FROM offenses WHERE pkey = {id}");
        public string GetOffense(decimal id) => GetSingleResult($"SELECT offense FROM offenses WHERE pkey = {id}").ToString();
        public string GetOffenseSpr(decimal id) => GetSingleResult($"SELECT nam FROM sproffense WHERE id = {id}").ToString();
        public void DeleteOffense() => SetQuery($"DELETE FROM offenses WHERE id = {OffenseID}");
        public List<File> GetFiles() => GetTList<File>($"SELECT id, nam, files FROM Files WHERE pkey = {Id}");
        public void DeleteFile() => SetQuery($"DELETE FROM files WHERE id = {FileID}");
        public bool EmployeeDublicate()
        {
            if (Mode.Button == ModeButton.Add)
            {
                int count = Convert.ToInt32(GetSingleResult($"SELECT count(*) FROM Offenders WHERE Employee = '{Employee}'"));
                if (count > 0)
                    return false;
                else return true;
            }
            else
                return true;
        }
        public void InsertOffenses(int idOffenders, string offense) => SetQuery($"INSERT INTO offenses (PKEY, offense) VALUES ({idOffenders}, '{offense}')");
        public void InsertFiles(byte[] bytes, string fileName, int idOffenders) => SetQuery($"INSERT INTO Files (PKEY, NAM, FILES) VALUES ({idOffenders},'{fileName}', :BLOB) RETURNING ID INTO :ID", bytes);
        public int InsertUpdateOffenders(byte[] bytePhoto)
        {
            if (Mode.Button == ModeButton.Add)
            {
                var id = SetQuery($@"INSERT INTO Offenders (EMPLOYEE, DOL, DR, DP, GRPPRG, REGION, SUMM, BALANCE, CUR, LOGINID, LOGINID_LASTEDIT, DATE_RECORD, DATE_LASTEDIT, STATUS, AUDITOR, DELETED, InBase, PHOTO) 
                                VALUES ('{Employee}','{Dol}',to_date('{Dr.ToShortDateString()}','dd.mm.yyyy'), to_date('{Dp.ToShortDateString()}','dd.mm.yyyy'),'{Grpprg}','{Region}',{Summ},{Balance}, '{Cur}', {Loginid}, {Loginid_lastedit}, to_date('{Date_record}','dd.mm.yyyy hh24:mi:ss'), to_date('{Date_lastedit}','dd.mm.yyyy hh24:mi:ss'), '{Status}', '{Auditor}', 'F','{Inbase}', :BLOB) RETURNING ID INTO :ID", bytePhoto);
                return id;
            }
            else
            {
                var id = SetQuery($@"UPDATE Offenders SET EMPLOYEE = '{Employee}', DOL = '{Dol}', DR = to_date('{Dr.ToShortDateString()}','dd.mm.yyyy'), DP = to_date('{Dp.ToShortDateString()}','dd.mm.yyyy'), GRPPRG = '{Grpprg}', REGION = '{Region}', SUMM = {Summ}, BALANCE = {Balance}, CUR = '{Cur}', LOGINID_LASTEDIT = {Loginid}, DATE_LASTEDIT = to_date('{Date_lastedit}','dd.mm.yyyy hh24:mi:ss'), STATUS = '{Status}', AUDITOR = '{Auditor}', InBase = '{Inbase}', PHOTO = :BLOB WHERE Id = {Id} RETURNING ID INTO :ID", bytePhoto);
                return id;
            }
        }
        public void DeleteOrRecovery() 
        {
            if (Mode.Button == ModeButton.MarkDelRec)
            {
                SetQuery($"UPDATE Offenders SET Deleted = '{Deleted}' WHERE Id = {Id}");
            }
            else if (Mode.Button == ModeButton.Delete)
            {

            }
        }
    }
}
