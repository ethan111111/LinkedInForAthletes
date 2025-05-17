using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recruitingWebApp.Models
{
    public class Measurments
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; } //foreign key 

        public string Measurement { get; set; }

        public string Value { get; set; }

    }
}
