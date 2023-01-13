using System.ComponentModel.DataAnnotations;

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
        public string? Type { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés státusza
        /// </summary>
        [Display(Name = "Igénylés státusza")]
        public string? Status { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés leírása
        /// </summary>
        [Display(Name = "Igénylés leírása")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Az igénylés leírása kötelező!")]
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Igényelt jogosultságok
        /// </summary>
        [Display(Name = "Igényelt jogosultságok")]
        [DataType(DataType.MultilineText)]
        public string? RequestedPermissions { get; set; } = string.Empty;

        /// <summary>
        /// Igénylés készítésének időpontja
        /// </summary>
        [Display(Name = "Igénylés készítésének időpontja")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}", ApplyFormatInEditMode = false)]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Igénylést feladó felhasználó
        /// </summary>
        [Display(Name = "Igénylést feladó felhasználó")]
        public Nullable<int> AuthorId { get; set; }

        [Display(Name = "Igénylést feladó felhasználó")]
        public virtual User? Author { get; set; }

        /// <summary>
        /// Igényléshez hozzátartozó osztály
        /// </summary>
        [Display(Name = "Igényléshez hozzátartozó osztály")]
        public Nullable<int> ClassId { get; set; }

        [Display(Name = "Igényléshez hozzátartozó osztály")]
        public virtual Class? Class { get; set; }

        /// <summary>
        /// Igénylés elintézésének ideje
        /// </summary>
        [Display(Name = "Igénylés elintézésének ideje")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. dddd, HH óra mm perc}", ApplyFormatInEditMode = false)]
        public DateTime ResolveDate { get; set; }
    }
}