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
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Jogosultság neve
        /// </summary>
        [Display(Name = "Jogosultság megnevezése")]
        [Required(ErrorMessage = "A jogosultság megnevezése kötelező!")]
        [MinLength(4, ErrorMessage = "A jogosultság neve nem lehet kevesebb mint 4 karakter")]
        [MaxLength(64, ErrorMessage = "A jogosultság neve nem lehet nagyobb mint 64 karakter")]
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// Jogosultság leírása
        /// </summary>
        [Display(Name = "Jogosultság leírása")]
        [Required(ErrorMessage = "A jogosultság leírásának megadása kötelező!")]
        [MinLength(4, ErrorMessage = "A jogosultság leírásának legalább 4 karakter hosszúnak kell lennie!")]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Az Active Directory-ban található jogosultsági nevek
        /// </summary>
        [Display(Name = "Active Directory-ban található jogosultsági nevek")]
        [Required(ErrorMessage = "Legalább 1 darab Active Directory jogosultság megadása kötelező!")]
        [MinLength(6, ErrorMessage = "Legalább egy darab jogosultságot írj be! (SKFB-) szükséges!")]
        public string? Permission { get; set; } = string.Empty;

        /// <summary>
        /// A jogosultság archiválva lett e
        /// </summary>
        [Display(Name = "Archivált")]
        public bool? Archived { get; set; } = false;
    }
}