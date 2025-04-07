using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recruitingWebApp.Models
{
    public class Post
    {
        [Key] 
        public int Id { get; set; }  // PostID

        [Required]
        public byte[] ImageData { get; set; } //post picture converted to bit array

        public string? Caption { get; set; }

        // Navigation property for User (One-to-One relationship)
        public virtual User User { get; set; }
    }
}
