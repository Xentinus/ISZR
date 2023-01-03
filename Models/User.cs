using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// A felhasználó adatai
    /// </summary>
    public class User
    {
        /// <summary>
        /// A felhasználó azonosítója ISZR-en belül
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A BV-ben használt felhasználóneve
        /// </summary>

        [Required(ErrorMessage = "Kérlek írd le a felhasználónevet!")]
        [MinLength(3, ErrorMessage = "A felhasználóné nem lehet kevesebb mint 3 karakter!")]
        [MaxLength(32, ErrorMessage = "A felhasználónév nem lehet nagyobb mint 32 karakter")]
        public string? Username { get; set; }

        /// <summary>
        /// A név amely megjelenik a szolgálati jegyeket
        /// </summary>
        [Required(ErrorMessage = "Kérlek írd le azt a nevet ami a szolgálati jegyeken megjelenjen!")]
        [MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
        [MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Felhasználó emailes elérhetősége
        /// </summary>
        public string? Email { get; set; } = string.Empty;

        /// <summary>
        /// Felhasználó telefonos elérhetősége
        /// </summary>
        public string? Phone { get; set; } = string.Empty;

        /// <summary>
        /// Rendfokozat
        /// </summary>
        [Required(ErrorMessage = "Kérlek válaszd ki a rendfokozatot!")]
        public string? Rank { get; set; }

        /// <summary>
        /// Osztály vagy alosztály azonosítója
        /// </summary>
        [Required(ErrorMessage = "Kérlek válaszd ki az osztályt!")]
        public int ClassId { get; set; }

        /// <summary>
        /// A felhasználó admin e
        /// </summary>
        public bool Administrator { get; } = false;

        /// <summary>
        /// Felhasználó utolsó bejelentkezésének ideje
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.Now;
    }
}