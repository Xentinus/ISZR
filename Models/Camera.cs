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
        [MaxLength(64, ErrorMessage = "A kamera neve nem lehet nagyobb mint 64 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kamera típusa
        /// </summary>
        [Display(Name = "Kamera típusa")]
        [Required(ErrorMessage = "A kamera típusának kiválasztása kötelező!")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Kamera helyszíne
        /// </summary>
        [Display(Name = "Kamera helyszíne")]
        [Required(ErrorMessage = "A kamera helyszínének megadása kötelező!")]
        [MinLength(2, ErrorMessage = "A kamera helyszíne nem lehet kevesebb mint 2 karakter")]
        [MaxLength(64, ErrorMessage = "A kamera helyszíne nem lehet nagyobb mint 64 karakter")]
        public string Sector { get; set; } = string.Empty;
    }
}