using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recruitingWebApp.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Caption { get; set; } = string.Empty;

        [Required]
        public string PostUrl { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
        public string ContentType { get; set; } = "application/octet-stream";

        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
