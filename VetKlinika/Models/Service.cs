using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Service
    {
        [Key]
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}