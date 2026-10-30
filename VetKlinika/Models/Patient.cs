using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace VetKlinika.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Species { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Breed { get; set; }

        [Range(0, 30)]
        public int Age { get; set; }

        public string? PhotoPath { get; set; }

        [NotMapped]
        public IFormFile? PhotoFile { get; set; }

        [Required]
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}