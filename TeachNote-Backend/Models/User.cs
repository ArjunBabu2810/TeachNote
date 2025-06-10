using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachNote_Backend.Models
{
    public class User{
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string role { get; set; }

        [Required]
        public string password { get; set; }

        public int departmentId { get; set; }

        [ForeignKey("departmentId")]
        public Department? Department {get; set;}
    }
}