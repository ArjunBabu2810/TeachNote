using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachNote_Backend.Models
{
    public class Subjects
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public int semester { get; set; }

        public int departmentId { get; set; }

        [ForeignKey("departmentId")]
        public Department? Department { get; set;  }

    }
}