using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Beosztás
    /// </summary>
    public class Position
    {
        public Position()
        {
            Users = new HashSet<User>();
        }

        /// <summary>
        /// Beosztás azonosítója
        /// </summary>
        [Key]
        public int PositionId { get; set; }

        /// <summary>
        /// Beosztás megnevezése
        /// </summary>
        [Display(Name = "Beosztás megnevezése")]
        [Required(ErrorMessage = "A beosztás megnevezése kötelező!")]
        [MinLength(3, ErrorMessage = "A beosztás neve nem lehet kevesebb mint 3 karakter")]
        [MaxLength(48, ErrorMessage = "A beosztás neve nem lehet nagyobb mint 48 karakter")]
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// Archiválva lett e a beosztás
        /// </summary>
        [Display(Name = "Archiválva lett e")]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Beosztáshoz hozzátartozó felhasználók
        /// </summary>
        public virtual ICollection<User> Users { get; set; }
    }
}