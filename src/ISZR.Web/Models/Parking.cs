using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
    /// <summary>
    /// Parkolási engedély
    /// </summary>
    public class Parking
    {
        /// <summary>
        /// Parkolási engedély azonosítója
        /// </summary>
        [Key]
        public int ParkingId { get; set; }

        /// <summary>
        /// Jármű márkája
        /// </summary>
        [Display(Name = "Jármű márkája")]
        public string? Brand { get; set; }

        /// <summary>
        /// Jármű modellje
        /// </summary>
        [Display(Name = "Jármű modellje")]
        [Required(ErrorMessage = "A modell megadása kötelező!")]
        [MaxLength(64, ErrorMessage = "A modell neve nem lehet nagyobb mint 64 karakter")]
        public string? Modell { get; set; }

        /// <summary>
        /// Rendszám
        /// </summary>
        [Display(Name = "Rendszám")]
        [Required(ErrorMessage = "A rendszám megadása kötelező!")]
        [MinLength(4, ErrorMessage = "A rendszám nem lehet kevesebb mint 4 karakter")]
        [MaxLength(32, ErrorMessage = "A rendszám nem lehet nagyobb mint 32 karakter")]
        public string? LicensePlate { get; set; }

        /// <summary>
        /// Hozzátartozó felhasználó
        /// </summary>
        [Display(Name = "Hozzátartozó felhasználó")]
        public Nullable<int> OwnerUserId { get; set; }

        [Display(Name = "Hozzátartozó felhasználó")]
        [ForeignKey("OwnerUserId")]
        public virtual User? OwnerUser { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}