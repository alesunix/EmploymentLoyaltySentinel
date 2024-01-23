using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EmploymentLoyaltySentinel.Models
{
    public class OffenseModel : BaseModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(100, ErrorMessage = "Количество символов не больше 100")]
        public string Nam { get; set; }
        public bool IsNew { get; set; }
        public string Deleted { get; set; }
        public DataTable GetDataOffense(string filter = null)
        {
            filter ??= string.Empty;
            return GetTable($"SELECT * FROM sproffense WHERE LOWER(nam) LIKE '%{filter.ToLower()}%' ORDER BY nam");
        }
        public DataTable GetRowToEdit(int id) => GetTable($"SELECT * FROM sproffense WHERE id = {id}");

        public void InsertUpdateOffense()
        {
            Nam = Nam.Replace("'", "\'\'");
            if (IsNew)
            {
                SetQuery($"INSERT INTO sproffense (Nam) VALUES ('{Nam}')");
            }
            else
            {
                SetQuery($"UPDATE sproffense SET Nam = '{Nam}' WHERE Id = {Id}");
            }
        }
        public void DeleteOrRecoveryOffense() => SetQuery($"UPDATE sproffense SET Deleted = '{Deleted}' WHERE Id = {Id}");

    }
}
