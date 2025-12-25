using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "请输入姓名")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "关联手机号")]
        [Phone(ErrorMessage = "请输入有效的手机号")]
        public string? OwnerPhone { get; set; }

        [Display(Name = "开始治疗日期")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "备注")]
        public string? Notes { get; set; }

        public List<TreatmentRecord> TreatmentRecords { get; set; } = new();
    }
}
