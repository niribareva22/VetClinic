using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Client
    {
        [Key]
        public int ClientID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}