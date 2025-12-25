using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "请输入手机号")]
        [Display(Name = "手机号")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }
    }
}
