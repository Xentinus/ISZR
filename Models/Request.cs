using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Models
{
    /// <summary>
    /// Az igénylés státuszai
    /// </summary>
    public enum requestStatus
    {
        Végrehajtva,
        Elutasítva,
        Folyamatban
    }

    /// <summary>
    /// Igénylések
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Az igénylés azonosító száma
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Igénylő felhasználó
        /// </summary>
        [Display(Name = "Igénylő személy")]
        [Required]
        public virtual User? Author { get; set; }

        /// <summary>
        /// Igénylő osztály
        /// </summary>
        [Display(Name = "Igénylő osztály")]
        [Required]
        public virtual Class? Class { get; set; }

        /// <summary>
        /// Az igénylés típusa
        /// </summary>
        [Display(Name = "Igénylés típusa")]
        [Required]
        public string? Type { get; set; } = string.Empty;

        /// <summary>
        /// Személyek akiknek az igénylés zajlik
        /// </summary>
        [Display(Name = "Kinek a részére")]
        [Required]
        public List<User>? ToWho { get; set; }

        /// <summary>
        /// Az igénylés leírása
        /// </summary>
        [Display(Name = "Igénylés leírása")]
        [Required]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Igényelt főnix 3 jogosultságok
        /// </summary>
        [Display(Name = "Igényelt főnix 3 jogosultságok")]
        public List<FonixPermission>? FonixPermissions { get; set; }

        /// <summary>
        /// Igényeld windows jogosultságok
        /// </summary>
        [Display(Name = "Igényelt windows jogosultságok")]
        public List<WindowsPermission>? WindowsPermissions { get; set; }

        /// <summary>
        /// Az igénylés feladásának ideje
        /// </summary>
        [Display(Name = "Igénylés feladásának ideje")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Az igénylés befejezézésnek időpontja
        /// </summary>
        [Display(Name = "Igénylés befejezésének időpontja")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FinishedDate { get; set; }

        /// <summary>
        /// Az igénylés státusza
        /// </summary>
        [Display(Name = "Igénylés státusza")]
        public requestStatus Status { get; set; } = (requestStatus)2;

        /// <summary>
        /// Amennyiben az igénylés el lett utasíva, annak magyarázata
        /// </summary>
        [Display(Name = "Lezárás magyarázata")]
        public string? StatusDesc { get; set; } = string.Empty;
    }
}