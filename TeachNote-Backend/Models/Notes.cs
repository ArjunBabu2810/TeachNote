using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachNote_Backend.Models
{
    public class Notes
    {
        [Key]
        public int id { get; set; }

        [Required]
        public DateTime postedDate { get; set; }

        public int subjectId { get; set; }

        [ForeignKey("subjectId")]
        public Subjects? Subjects { get; set; }

        public string description { get; set; }

        [Required]
        public string pdfFile { get; set; }


    }
}