using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请输入手机号")]
        [Phone(ErrorMessage = "请输入有效的手机号")]
        [Display(Name = "手机号")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的11位手机号码")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string? ConfirmPassword { get; set; }
    }
}
