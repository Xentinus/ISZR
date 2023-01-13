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
        public int WindowsPermissionId { get; set; }

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
        /// Jogosultság készítésének időpontja
        /// </summary>
        [Display(Name = "Jogosultság készítésének időpontja")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}", ApplyFormatInEditMode = false)]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Utoljára szerkesztette
        /// </summary>
        [Display(Name = "Utoljára szerkesztette")]
        public Nullable<int> UserId { get; set; }

        public virtual User? User { get; set; }

        /// <summary>
        /// Jogosultság utolsó szerkesztésének ideje
        /// </summary>
        [Display(Name = "Jogosultság utolsó szerkesztésének ideje")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}", ApplyFormatInEditMode = false)]
        public DateTime ModifyDate { get; set; } = DateTime.Now;
    }
}