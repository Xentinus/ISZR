using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Főnix 3 jogosultság
    /// </summary>
    public class FonixPermission
    {
        /// <summary>
        /// Jogosultság azonosítója
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Jogosultság neve
        /// </summary>
        [Required(ErrorMessage = "Kérlek nevezd el a jogosultságot!")]
        [MinLength(4, ErrorMessage = "A jogosultság nevének legalább 4 karakter hosszúnak kell lennie!")]
        public string? Name { get; set; }

        /// <summary>
        /// Jogosultság leírása
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj leírást a jogosultságról!")]
        [MinLength(5, ErrorMessage = "A jogosultság leírásának legalább 5 karakter hosszúnak kell lennie!")]
        public string? Description { get; set; }

        /// <summary>
        /// Van e országos megtekintő beállítása a jogosultságnak
        /// </summary>
        public bool CountryView { get; set; } = true;

        /// <summary>
        /// Van e intézeti megtekintő beállítása a jogosultságnak
        /// </summary>
        public bool View { get; set; } = true;

        /// <summary>
        /// Van e intézeti szerkesztő beállítása a jogosultságnak
        /// </summary>
        public bool Edit { get; set; } = true;
    }
}