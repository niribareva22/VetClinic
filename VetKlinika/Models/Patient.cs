using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinika.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Species { get; set; } = string.Empty;

        public int Age { get; set; }

        [Required]
        public int ClientID { get; set; }

        [ForeignKey("ClientID")]
        public virtual Client? Client { get; set; }
    }
}