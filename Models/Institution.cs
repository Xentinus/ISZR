using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Models
{
    /// <summary>
    /// Intézet
    /// </summary>
    public class Institution
    {
        public Institution()
        {
            Users = new HashSet<User>();
        }

        /// <summary>
        /// Intézet azonosítója
        /// </summary>
        [Key]
        public int InstitutionId { get; set; }

        /// <summary>
        /// Intézet megnevezése
        /// </summary>
        [Display(Name = "Intézet megnevezése")]
        [Required(ErrorMessage = "Az intézet megadása kötelező!")]
        [MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
        [MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Intézet rövidítésének megnevezése
        /// </summary>
        [Display(Name = "Intézet rövidítése")]
        [Required(ErrorMessage = "Az intézet rövidítésének megadása kötelező!")]
        [MinLength(2, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 2 karakter")]
        [MaxLength(10, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 10 karakter")]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Intézethez hozzátartozó agglomeráció
        /// </summary>
        [Display(Name = "Intézet agglomerációja")]
        [Required(ErrorMessage = "Az agglomeráció kiválasztása kötelező!")]
        [DefaultValue(null)]
        public Nullable<int> AgglomerationId { get; set; }

        [Display(Name = "Intézet agglomerációja")]
        [ForeignKey("AgglomerationId")]
        public virtual Agglomeration? Agglomeration { get; set; }

        /// <summary>
        /// Intézet vezetője
        /// </summary>
        [Display(Name = "Intézet vezetője")]
        [Required(ErrorMessage = "Az intézet vezetőjének kiválasztása kötelező!")]
        [DefaultValue(null)]
        public Nullable<int> UserId { get; set; }

        [Display(Name = "Intézet vezetője")]
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Intézet felhasználói
        /// </summary>
        [Display(Name = "Intézet felhasználói")]
        [InverseProperty("Users")]
        public virtual ICollection<User> Users { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}