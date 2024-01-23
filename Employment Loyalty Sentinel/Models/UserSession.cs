using System.ComponentModel.DataAnnotations;

namespace EmploymentLoyaltySentinel.Models
{
    public class UserSession : BaseModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [MinLength(3, ErrorMessage = "Пароль должен содержать не менее 3 символов")]
        public string Pass1 { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [MinLength(3, ErrorMessage = "Пароль должен содержать не менее 3 символов")]
        public string Pass2 { get; set; }
        public string Password { get; set; }
        UserModel uModel = new();
        public bool ChangePassword()
        {
            if (Id < 0)
                return false;
            if (Pass1 == Pass2)
            {
                Password = uModel.HashPassword(Pass2);
                SetQuery($"UPDATE userid SET Password = '{Password}' WHERE Id = {Id}");
                return true;
            }
            else
            {
                MyMessage = "Пароли не совпадают!";
                return false;
            }
        }
        public string GetLogin()
        {
            return GetSingleResult($@"SELECT login FROM userid WHERE id = {Id}").ToString();
        }
    }
}
