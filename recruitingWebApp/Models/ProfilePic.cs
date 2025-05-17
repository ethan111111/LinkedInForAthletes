using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recruitingWebApp.Models
{
    public class ProfilePic
    {
        [Key] 
        public int Id { get; set; }  

        [Required]
        public byte[] ImageData { get; set; } //image converted to bit array and stored

        // Navigation property for User (One-to-One relationship)
        public virtual User User { get; set; }
    }
}
