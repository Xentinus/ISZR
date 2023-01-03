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
        public int Id { get; set; }

        /// <summary>
        /// Kamera neve
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj nevet a kamerának!")]
        [MinLength(2, ErrorMessage = "A kamera nevének legalább 2 karakter hosszúnak kell lennie!")]
        public string? Name { get; set; }

        /// <summary>
        /// Kamera típusa
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Kamera elhelyezkedése
        /// </summary>
        [Required(ErrorMessage = "Kérlek írd le a kamera elhelyezkedését!")]
        [MinLength(2, ErrorMessage = "A kamera szektor nevének legalább 2 karakter hosszúnak kell lennie!")]
        public string? Sector { get; set; }
    }
}