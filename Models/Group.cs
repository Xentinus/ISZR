using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
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
        [Required(ErrorMessage = "A csoport megnevezése kötelező!")]
        [MinLength(2, ErrorMessage = "A csoport megnevezése nem lehet kevesebb mint 2 karakter")]
        [MaxLength(48, ErrorMessage = "A csoport megnevezése nem lehet nagyobb mint 48 karakter")]
        public string? Name { get; set; }

        /// <summary>
        /// Csoporthoz tartozó osztály
        /// </summary>
        [Display(Name = "Csoport osztálya")]
        [Required(ErrorMessage = "Az osztály kiválasztása kötelező!")]
        public Nullable<int> ClassId { get; set; }

        public Class? Class { get; set; }

        /// <summary>
        /// Csoporthoz hozzátartozó jogosultságok
        /// </summary>
        [Display(Name = "Csoport jogosultságai")]
        public ICollection<GroupPermissions>? Permissions { get; set; }

        public Group()
        {
            Permissions = new Collection<GroupPermissions>();
        }
    }
}