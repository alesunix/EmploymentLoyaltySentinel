using EmploymentLoyaltySentinel.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace EmploymentLoyaltySentinel.Models
{
    public class UserModel : BaseModel
    {
        public bool IsNew { get; set; }
        public int Id { get; set; }
        public string Password { get; set; }
        //-----------------------------------
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(100, ErrorMessage = "Количество символов не больше 100")]
        public string Employee { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(12, ErrorMessage = "Количество символов не больше 12")]
        public string Login { get; set; }
        [RequiredIf(nameof(IsNew), true, ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass1 { get; set; }
        [RequiredIf(nameof(IsNew), true, ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass2 { get; set; }
        [StringLength(25, ErrorMessage = "Количество символов не больше 25")]
        public string Mail { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public int PService { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [Range(1, 9, ErrorMessage = "Поле не должно быть пустым!")]
        public int RoleId { get; set; }
        public string Deleted { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Region { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Grpprg { get; set; }
        public DataTable GetDataUsers(string filter = null)
        {
            filter ??= string.Empty;
            return GetTable($@"SELECT a.id, a.employee, a.login, a.Deleted, b.nam as grpprg, c.name as role
                                FROM userid a
                                left join sprgrpprg_v b on a.grpprg = b.grpprg 
                                left join sprroles c on a.role_id = c.id
                                WHERE LOWER(employee) LIKE '%{filter.ToLower()}%' 
                                ORDER BY employee");
        }
        public List<Employee> GetListEmployee(string filter = null)
        {
            filter ??= string.Empty;
            return GetTList<Employee>($@"SELECT a.fio, a.otv, a.dol, a.lev as region, a.grpprg, a.dr, a.de
                                    FROM sprotv_mv a
                                    WHERE de = to_date('01.01.2100','dd.mm.yyyy') AND rownum <= 200 AND LOWER(a.fio) LIKE '%{filter.ToLower()}%' ORDER BY a.fio");
        }
        public Dictionary<decimal, string> GetListRoles() => GetDictionary<decimal, string>("SELECT id, name FROM sprroles ORDER BY name");
        public Dictionary<string, string> GetListRegion() => GetDictionary<string, string>("SELECT key, nam FROM sprregion_v ORDER BY nam");
        public Dictionary<string, string> GetListGrpprg(string filter = null)
        {
            filter ??= string.Empty;
            return GetDictionary<string, string>($"SELECT grpprg, grpprg || ' - ' || nam as nam FROM sprgrpprg_v WHERE con = 'T' AND LOWER(nam) LIKE '%{filter.ToLower()}%' ORDER BY nam");
        }
        public bool LoginDublicate()
        {
            int count = Convert.ToInt32(GetSingleResult($"SELECT count(*) FROM userid where login = '{Login}'"));
            if (count > 0)
            {
                MyMessage = "Пользователь с таким именем уже существует в базе!";
                return false;
            }
            return true;
        }
        public bool AddOrEditUser()
        {
            if (!string.IsNullOrWhiteSpace(Pass1) & !string.IsNullOrWhiteSpace(Pass2))
            {
                if (Pass1 == Pass2)
                {
                    Password = HashPassword(Pass2);
                    return InsertOrUpdate();
                }
                else
                {
                    MyMessage = "Пароли не совпадают!";
                    return false;
                }
            }
            else
            {
                /// Если поля пустые то не обновляем пароль
                SetQuery($@"UPDATE userid SET Employee = '{Employee}', Login = '{Login}', Mail = '{Mail}', PService = {PService}, Region = '{Region}', Grpprg = '{Grpprg}',
                                              ROLE_ID = {RoleId} WHERE Id = {Id}");
                return true;
            }
        }
        private bool InsertOrUpdate()
        {
            if (Mode.Button == ModeButton.Add)
            {
                if (LoginDublicate())
                {
                    SetQuery($@"INSERT INTO userid (Employee, Login, Password, Mail, PService, Deleted, Region, Grpprg, ROLE_ID) 
                                VALUES ('{Employee}','{Login}','{Password}','{Mail}',{PService},'F','{Region}','{Grpprg}', {RoleId})");
                    return true;
                }
                else return false;/// Если пользователь с таким именем существует
            }
            else
            {
                SetQuery($@"UPDATE userid SET Employee = '{Employee}', Login = '{Login}', Password = '{Password}', Mail = '{Mail}', PService = {PService},
                                              Region = '{Region}', Grpprg = '{Grpprg}', ROLE_ID = {RoleId} WHERE Id = {Id}");
                return true;
            }
        }
        public string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(SHA256.HashData(bytes)).ToUpper();
        }
        public void DeleteOrRecovery()
        {
            if (Mode.Button == ModeButton.MarkDelRec)
            {
                SetQuery($"UPDATE userid SET Deleted = '{Deleted}' WHERE Id = {Id}");
            }
            else if (Mode.Button == ModeButton.Delete)
            {

            }
        }
        public DataTable GetRowToEdit(int id) => GetTable($"SELECT * FROM userid WHERE id = {id}");

    }
}
