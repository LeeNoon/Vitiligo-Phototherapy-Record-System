using System.ComponentModel.DataAnnotations;

namespace VitiligoTracker.Models
{
    public class BodyPartDict
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "部位名称")]
        public string Name { get; set; } = string.Empty;
    }
}
