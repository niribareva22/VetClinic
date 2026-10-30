using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Range(0.01, 10000.00)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}