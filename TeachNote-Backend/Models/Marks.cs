using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachNote_Backend.Models
{
    public class Marks
    {
        [Key]
        public int id { get; set; }

        [Required]
        public int subjectId { get; set; }

        [Required]
        public int userId { get; set; }

        public float internal1 { get; set; }

        public float internal2 { get; set; }

        public float external { get; set; }

        [ForeignKey("subjectId")]
        public Subjects? Subjects { get; set; }

        [ForeignKey("userId")]
        public User? User { get; set; }


    }
}