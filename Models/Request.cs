using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Models
{
    /// <summary>
    /// Igénylés
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Igénylés azonosítója
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// Igénylés típusa
        /// </summary>
        [Display(Name = "Igénylés típusa")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés státusza
        /// </summary>
        [Display(Name = "Igénylés státusza")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés leírása
        /// </summary>
        [Display(Name = "Igénylés leírása")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Igényelt jogosultságok
        /// </summary>
        [Display(Name = "Igényelt jogosultságok")]
        [DataType(DataType.MultilineText)]
        public string RequestedPermissions { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés készítésének időpontja
        /// </summary>
        [Display(Name = "Igénylés készítésének időpontja")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Igénylést feladó felhasználó
        /// </summary>
        [Display(Name = "Igénylést feladó felhasználó")]
        public Nullable<int> RequestAuthorId { get; set; }

        [Display(Name = "Igénylést feladó felhasználó")]
        [ForeignKey("RequestAuthorId")]
        public virtual User? RequestAuthor { get; set; }

        /// <summary>
        /// Személy aki számára az igénylés zajlik
        /// </summary>
        [Display(Name = "Személy aki számára az igénylés zajlik")]
        public Nullable<int> RequestForId { get; set; }

        [Display(Name = "Személy aki számára az igénylés zajlik")]
        [ForeignKey("RequestForId")]
        public virtual User? RequestFor { get; set; }

        /// <summary>
        /// Igénylés elintézésének ideje
        /// </summary>
        [Display(Name = "Igénylés elintézésének ideje")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}")]
        public DateTime ResolveDate { get; set; }
    }
}