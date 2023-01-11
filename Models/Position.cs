using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Beosztás
    /// </summary>
    public class Position
    {
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
        /// Beosztáshoz hozzátartozó felhasználók (OneToMany)
        /// </summary>
        public virtual ICollection<User>? Users { get; set; }

        public Position()
        {
            Users = new Collection<User>();
        }
    }
}