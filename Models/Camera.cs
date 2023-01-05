using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Intézeti kamera
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Kamera azonosítója
        /// </summary>
        [Key]
        public int CameraId { get; set; }

        /// <summary>
        /// Kamera neve
        /// </summary>
        [Display(Name = "Kamera megnevezése")]
        [Required(ErrorMessage = "A kamera nevének megadása kötelező!")]
        [MinLength(2, ErrorMessage = "A kamera neve nem lehet kevesebb mint 2 karakter")]
        [MaxLength(48, ErrorMessage = "A kamera neve nem lehet nagyobb mint 48 karakter")]
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// Kamera típusa
        /// </summary>
        [Display(Name = "Kamera típusa")]
        [Required(ErrorMessage = "A kamera típusának kiválasztása kötelező!")]
        public string? Type { get; set; } = string.Empty;

        /// <summary>
        /// Kamera szektora
        /// </summary>
        [Display(Name = "Kamera szektora")]
        [Required(ErrorMessage = "A kamera szektorának megadása kötelező!")]
        [MinLength(2, ErrorMessage = "A kamera szektora nem lehet kevesebb mint 2 karakter")]
        [MaxLength(48, ErrorMessage = "A kamera szektora nem lehet nagyobb mint 48 karakter")]
        public string? Sector { get; set; } = string.Empty;
    }
}