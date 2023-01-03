using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Osztály/Alosztály
    /// </summary>
    public class Class
    {
        /// <summary>
        /// Az osztály azonosítója ISZR-en belül
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Az osztály neve
        /// </summary>
        [Required(ErrorMessage = "Kérlek adj nevet az osztálynak!")]
        [MinLength(3, ErrorMessage = "Az osztály nevének legalább 3 karakter hosszúnak kell lennie!")]
        public string? Name { get; set; }

        /// <summary>
        /// Az osztályt vezető neve
        /// </summary>
        [Required(ErrorMessage = "Kérlek nevezd meg az osztály vezetőjét!")]
        [MinLength(3, ErrorMessage = "Az osztályvezető neve legalább 3 karakter hosszúnak kell lennie")]
        public string? LeaderName { get; set; }

        /// <summary>
        /// Az osztályt vezető rendfokozata
        /// </summary>
        public string? LeaderRank { get; set; }
    }
}