using System.ComponentModel.DataAnnotations;

namespace ToyStore.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email/Username là bắt buộc")]
        [Display(Name = "Email/Username")]
        public string EmailOrUsername { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        public string UserType { get; set; } = "Customer"; // Customer, Admin, Staff
    }
}




