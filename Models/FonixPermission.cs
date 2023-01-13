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
        [Key]
        public int FonixPermissionId { get; set; }

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
        [MinLength(5, ErrorMessage = "A jogosultság leírásának legalább 5 karakter hosszúnak kell lennie!")]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Van e országos megtekintő beállítása a jogosultságnak
        /// </summary>
        [Display(Name = "Országos megtekintő")]
        public bool CountryView { get; set; } = true;

        /// <summary>
        /// Van e intézeti megtekintő beállítása a jogosultságnak
        /// </summary>
        [Display(Name = "Intézeti megtekintő")]
        public bool View { get; set; } = true;

        /// <summary>
        /// Van e intézeti szerkesztő beállítása a jogosultságnak
        /// </summary>
        [Display(Name = "Intézeti szerkesztő")]
        public bool Edit { get; set; } = true;

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