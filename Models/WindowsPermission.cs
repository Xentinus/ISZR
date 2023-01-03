using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Windows jogosultság
    /// </summary>
    public class WindowsPermission
    {
        /// <summary>
        /// Jogosultság azonosítója
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Jogosultság neve
        /// </summary>
        [Required(ErrorMessage = "Kérlek nevezd el a jogosultságot!")]
        [MinLength(3, ErrorMessage = "A jogosultság nevének legalább 3 karakter hosszúnak kell lennie!")]
        public string? Name { get; set; }

        /// <summary>
        /// Jogosultság leírása
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj hozzá leírást!")]
        [MinLength(4, ErrorMessage = "A jogosultság leírásának legalább 4 karakter hosszúnak kell lennie!")]
        public string? Description { get; set; }

        /// <summary>
        /// Az Active Directory-ban található nevek
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj meg legalább egy darab Active Directory jogosultságot!")]
        [MinLength(6, ErrorMessage = "Legalább egy darab jogosultságot írj be! (SKFB-) szükséges!")]
        public string? Permission { get; set; }
    }
}