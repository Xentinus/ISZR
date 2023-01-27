using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Models
{
    /// <summary>
    /// Agglomeráció
    /// </summary>
    public class Agglomeration
    {
        public Agglomeration()
        {
            Institutions = new HashSet<Institution>();
        }

        /// <summary>
        /// Agglomeráció azonosítója
        /// </summary>
        [Key]
        public int AgglomerationId { get; set; }

        /// <summary>
        /// Agglomeráció megnevezése
        /// </summary>
        [Display(Name = "Agglomeráció megnevezése")]
        [Required(ErrorMessage = "Az agglomeráció megadása kötelező!")]
        [MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
        [MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Agglomerációhoz tartozó intézetek
        /// </summary>
        [Display(Name = "Agglomerációhoz tartozó intézetek")]
        [InverseProperty("Institutions")]
        public virtual ICollection<Institution> Institutions { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}