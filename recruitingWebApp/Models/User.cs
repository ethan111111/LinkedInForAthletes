using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace recruitingWebApp.Models
{
    public class User 
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        
        [Required]
        [MaxLength(50)]
        public string Password { get; set; }


        public string? Bio { get; set; }

        // Foreign Key
        public int? ProfilePicId { get; set; }  // Changed to match ProfilePic.Id

        [ForeignKey("ProfilePicId")]
        public virtual ProfilePic ProfileImage { get; set; }
    }
}
