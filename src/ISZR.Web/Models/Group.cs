using System.ComponentModel.DataAnnotations;

namespace ISZR.Web.Models
{
    /// <summary>
    /// Jogosultsági csoport
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Csoport azonosítója
        /// </summary>
        [Key]
        public int GroupId { get; set; }

        /// <summary>
        /// Csoport megnevezése
        /// </summary>
        [Display(Name = "Csoport megnevezése")]
        [Required(ErrorMessage = "A csoportnév megadása kötelező!")]
        [MinLength(3, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 3 karakter")]
        [MaxLength(64, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 64 karakter")]
        public string? Name { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Csoporthoz hozzátartozó jogosultságok
        /// </summary>
        public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    }
}