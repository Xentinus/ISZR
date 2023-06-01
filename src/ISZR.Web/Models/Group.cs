using System.ComponentModel.DataAnnotations;

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
		[MinLength(3, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 3 karakter")]
		[MaxLength(64, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 64 karakter")]
		public string? Name { get; set; }

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