using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models
{
    public class TreatmentRecord
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Display(Name = "治疗日期")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "治疗部位")]
        public string? BodyPart { get; set; }

        [Display(Name = "照射剂量 (mJ/cm²)")]
        [Required(ErrorMessage = "请输入照射剂量")]
        public double IrradiationDose { get; set; }

        [Display(Name = "累积剂量 (mJ/cm²)")]
        public double CumulativeDose { get; set; }

        [Display(Name = "时长 (秒)")]
        public int? DurationSeconds { get; set; }

        [Display(Name = "治疗后反应/备注")]
        public string? Reaction { get; set; }
    }
}
