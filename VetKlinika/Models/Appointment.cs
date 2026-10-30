using System;
using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        [Required]
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        [Required]
        public int PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        [Required]
        public int VetId { get; set; }
        public virtual Vet? Vet { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public virtual Service? Service { get; set; }
    }
}