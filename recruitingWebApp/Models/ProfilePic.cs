using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recruitingWebApp.Models
{
    public class ProfilePic
    {
        [Key] // Ensure this is marked as the primary key
        public int Id { get; set; }  // Changed from ProfileID to Id (by EF convention)

        [Required]
        public byte[] ImageData { get; set; }

        // Navigation property for User (One-to-One relationship)
        public virtual User User { get; set; }
    }
}
