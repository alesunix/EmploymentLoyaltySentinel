namespace EmploymentLoyaltySentinel.Models
{
    public class ReviseModel : BaseModel
    {
        public object Photo { get; set; }
        public string Fio { get; set; }
        public DateTime Dr { get; set; }
        public string Dol { get; set; }
        public string Grpprg { get; set; }
        public string Region { get; set; }
        public DateTime De { get; set; }
        public List<ReviseModel> GetNotFired(string filter = null)
        {
            filter ??= string.Empty;
            var list = GetTList<ReviseModel>($@"SELECT c.photo, a.fio, a.dr, a.dol, b.nam as grpprg, d.nam as region, a.de
                                                  FROM sprotv_mv a 
                                                  left join sprgrpprg_v b on a.grpprg = b.grpprg 
                                                  left join offenders c on a.fio = c.employee
                                                  left join sprregion_v d on c.region = d.key
                                                  WHERE a.fio = c.employee AND a.dr = c.dr AND LOWER(c.Employee) LIKE '%{filter.ToLower()}%'
                                                  AND de > sysdate");
            return list;
        }
    }
}
