using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Jogosultsági csoport
	/// </summary>
	public class Group
	{
		/// <summary>
		/// Csoport azonosítója
		/// </summary>
		[Key]
		public int GroupId { get; set; }

		/// <summary>
		/// Csoport megnevezése
		/// </summary>
		[Display(Name = "Csoport megnevezése")]
		[Required(ErrorMessage = "A csoportnév megadása kötelező!")]
		[MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
		[MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
		public string? Name { get; set; }

		/// <summary>
		/// Csoport osztálya
		/// </summary>
		[Display(Name = "Csoport osztálya")]
		[Required(ErrorMessage = "Az osztály kiválasztása kötelező!")]
		public Nullable<int> ClassId { get; set; }

		[Display(Name = "Csoport osztálya")]
		public virtual Class? Class { get; set; }

		/// <summary>
		/// Csoporthoz hozzátartozó Windows jogosultságok
		/// </summary>
		[Display(Name = "Csoporthoz hozzátartozó Windows jogosultságok")]
		[DataType(DataType.MultilineText)]
		public string? WindowsPermissions { get; set; }

		/// <summary>
		/// Csoporthoz hozzátartozó Főnix 3 jogosultságok
		/// </summary>
		[Display(Name = "Csoporthoz hozzátartozó Főnix 3 jogosultságok")]
		[DataType(DataType.MultilineText)]
		public string? FonixPermissions { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}
