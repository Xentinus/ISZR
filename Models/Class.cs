using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Osztály/Alosztály
    /// </summary>
    public class Class
    {
        public Class()
        {
            Users = new HashSet<User>();
            Requests = new HashSet<Request>();
        }

        /// <summary>
        /// Az osztály azonosítója ISZR-en belül
        /// </summary>
        [Key]
        public int ClassId { get; set; }

        /// <summary>
        /// Az osztály neve
        /// </summary>
        [Display(Name = "Osztály megnevezése")]
        [Required(ErrorMessage = "Az osztály nevének megadása kötelező!")]
        [MinLength(3, ErrorMessage = "Az osztály neve nem lehet kevesebb mint 3 karakter")]
        [MaxLength(64, ErrorMessage = "Az osztály neve nem lehet nagyobb mint 64 karakter")]
        public string? Name { get; set; } = string.Empty;

        /// <summary>
        /// Archiválva lett e az osztály
        /// </summary>
        [Display(Name = "Archiválva lett e")]
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Osztályhoz hozzátartozó felhasználók
        /// </summary>
        public virtual ICollection<User> Users { get; set; }

        /// <summary>
        /// Osztályhoz hozzátartozó igénylések
        /// </summary>
        public virtual ICollection<Request> Requests { get; set; }
    }
}