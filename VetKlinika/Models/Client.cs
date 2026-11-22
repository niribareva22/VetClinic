using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VetKlinika.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Login account")]
        public string? IdentityUserId { get; set; }

        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}