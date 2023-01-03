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
        public int Id { get; set; }

        /// <summary>
        /// Beosztás megnevezése
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj nevet a beosztásnak!")]
        [MinLength(3, ErrorMessage = "A beosztásnak legalább 3 karakter hosszúnak kell lennie!")]
        public string? Name { get; set; }
    }
}