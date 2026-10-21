using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinika.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient Patient { get; set; } = null!;

        [ForeignKey("Vet")]
        public int VetID { get; set; }
        public Vet Vet { get; set; } = null!;

        [ForeignKey("Service")]
        public int ServiceID { get; set; }
        public Service Service { get; set; } = null!;

        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}