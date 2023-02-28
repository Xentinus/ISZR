using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
    /// <summary>
    /// Osztály/Alosztály
    /// </summary>
    public class Class
    {
        /// <summary>
        /// Osztály azonosítója
        /// </summary>
        [Key]
        public int ClassId { get; set; }

        /// <summary>
        /// Osztály megnevezése
        /// </summary>
        [Display(Name = "Osztály megnevezése")]
        [Required(ErrorMessage = "Az osztály nevének megadása kötelező!")]
        [MinLength(3, ErrorMessage = "Az osztály neve nem lehet kevesebb mint 3 karakter")]
        [MaxLength(64, ErrorMessage = "Az osztály neve nem lehet nagyobb mint 64 karakter")]
        public string? Name { get; set; }

        /// <summary>
        /// Személy aki az osztály jogosultságait engedélyezi
        /// </summary>
        [Display(Name = "Személy aki az osztály jogosultságait engedélyezi")]
        public Nullable<int> AuthorizerId { get; set; }

        [Display(Name = "Személy aki az osztály jogosultságait engedélyezi")]
        [ForeignKey("AuthorizerId")]
        public virtual User? Authorizer { get; set; }

        /// <summary>
        /// Osztályhoz hozzátartozó felhasználók
        /// </summary>
        public virtual List<User>? Users { get; set; }

        /// <summary>
        /// Osztályhoz hozzátartozó csoportok
        /// </summary>
        public virtual List<Group>? Groups { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}