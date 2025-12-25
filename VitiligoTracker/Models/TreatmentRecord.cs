using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models
{
    public class TreatmentRecord
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

    [Display(Name = "治疗日期时间")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "治疗部位")]
        public string? BodyPart { get; set; }

    [Display(Name = "照射剂量 (mJ/cm²)")]
    [Required(ErrorMessage = "请输入照射剂量")]
    public double IrradiationDose { get; set; }

    [Display(Name = "建议剂量 (mJ/cm²)")]
    public double? SuggestDose { get; set; }

        [Display(Name = "累积剂量 (mJ/cm²)")]
        public double CumulativeDose { get; set; }

    // 已去除时长字段

        [Display(Name = "治疗后反应")]
        public ReactionType Reaction { get; set; } = ReactionType.None;
    }

    public enum ReactionType
    {
        [Display(Name = "无反应")]
        None = 0,
        [Display(Name = "红斑反应")]
        Erythema = 1,
        [Display(Name = "起水泡")]
        Blister = 2
    }
}
