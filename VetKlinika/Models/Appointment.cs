using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinika.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        [Required]
        public int PatientID { get; set; }
        [ForeignKey("PatientID")]
        public virtual Patient? Patient { get; set; }

        [Required]
        public int VetID { get; set; }
        [ForeignKey("VetID")]
        public virtual Vet? Vet { get; set; }

        [Required]
        public int ServiceID { get; set; }
        [ForeignKey("ServiceID")]
        public virtual Service? Service { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }
}