using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Intézet
    /// </summary>
    public class Institution
    {
        /// <summary>
        /// Intézet azonosítója
        /// </summary>
        [Key]
        public int InstitutionId { get; set; }

        /// <summary>
        /// Intézet megnevezése
        /// </summary>
        [Display(Name = "Intézet megnevezése")]
        [Required(ErrorMessage = "Az intézet megadása kötelező!")]
        [MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
        [MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Intézet rövidítésének megnevezése
        /// </summary>
        [Display(Name = "Intézet rövidítése megnevezése")]
        [Required(ErrorMessage = "Az intézet rövidítésének megadása kötelező!")]
        [MinLength(2, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 2 karakter")]
        [MaxLength(10, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 10 karakter")]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}