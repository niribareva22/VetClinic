using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Vet
    {
        [Key]
        public int VetID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}