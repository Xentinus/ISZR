using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Osztály/Alosztály
    /// </summary>
    public class Class
    {
        /// <summary>
        /// Az osztály azonosítója ISZR-en belül
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Az osztály neve
        /// </summary>
        [Display(Name = "Osztály megnevezése")]
        [Required(ErrorMessage = "Az osztály nevének megadása kötelező!")]
        [MinLength(3, ErrorMessage = "Az osztály neve nem lehet kevesebb mint 3 karakter")]
        [MaxLength(64, ErrorMessage = "Az osztály neve nem lehet nagyobb mint 64 karakter")]
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// Az osztályt vezető neve
        /// </summary>
        [Display(Name = "Osztályvezető neve")]
        [Required(ErrorMessage = "Az osztályvezető nevének megadása kötelező!")]
        [MinLength(4, ErrorMessage = "Az osztályvezető neve nem lehet kevesebb mint 4 karakter")]
        [MaxLength(64, ErrorMessage = "Az osztályvezető neve nem lehet nagyobb mint 64 karakter")]
        public string? LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// Az osztályt vezető rendfokozata
        /// </summary>
        [Display(Name = "Osztályvezető rendfokozata")]
        [Required(ErrorMessage = "Az osztályvezető rendfokozatának kiválasztása kötelező!")]
        public string? LeaderRank { get; set; } = string.Empty;

        // Relationships

        /// <summary>
        /// Osztályhoz hozzátartozó felhasználók
        /// </summary>
        public List<User>? Users { get; set; }

        /// <summary>
        /// Osztályhoz hozzátartozó igénylések
        /// </summary>
        public List<Request>? Requests { get; set; }
    }
}