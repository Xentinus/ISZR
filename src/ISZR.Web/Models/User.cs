using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
	/// <summary>
	/// A felhasználó adatai
	/// </summary>
	public class User
	{
		/// <summary>
		/// A felhasználó azonosítója ISZR-en belül
		/// </summary>
		[Key]
		public int UserId { get; set; }

		/// <summary>
		/// A BV-ben használt felhasználóneve
		/// </summary>
		[Display(Name = "Felhasználónév")]
		public string? Username { get; set; }

		/// <summary>
		/// A név amely megjelenik a szolgálati jegyeket
		/// </summary>
		[Display(Name = "Név")]
		[Required(ErrorMessage = "A név megadása kötelező!")]
		[MinLength(4, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 4 karakter")]
		[MaxLength(32, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 32 karakter")]
		public string? DisplayName { get; set; }

		/// <summary>
		/// Rendfokozat
		/// </summary>
		[Display(Name = "Rendfokozat")]
		[Required(ErrorMessage = "A rendfokozat kiválasztása kötelező!")]
		public string? Rank { get; set; }

		/// <summary>
		/// Felhasználó emailes elérhetősége
		/// </summary>
		[Display(Name = "E-mail cím")]
		[Required(AllowEmptyStrings = true)]
		[EmailAddress(ErrorMessage = "A mezőbe beírt cím nem e-mail cím!")]
		public string? Email { get; set; }

		/// <summary>
		/// Felhasználó telefonos elérhetősége
		/// </summary>
		[Display(Name = "NTG elérhetőség")]
		[Required(AllowEmptyStrings = true)]
		[MaxLength(7, ErrorMessage = "Az NTG elérhetőség nem lehet nagyobb mint 7 karakter")]
		public string? Phone { get; set; }

		/// <summary>
		/// Felhasználó utolsó bejelentkezésének ideje
		/// </summary>
		[Display(Name = "Utolsó bejelentkezés")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. HH:mm}")]
		public DateTime LastLogin { get; set; } = DateTime.Now;

		/// <summary>
		/// Felhasználó bejelentkezésének mennyisége
		/// </summary>
		[Display(Name = "Bejelentkezések mennyisége")]
		public int LogonCount { get; set; } = 0;

		/// <summary>
		/// Felhasználóhoz osztálya
		/// </summary>
		[Display(Name = "Felhasználó osztálya")]
		[Required(ErrorMessage = "Az osztály kiválasztása kötelező!")]
		public Nullable<int> ClassId { get; set; }

		[Display(Name = "Felhasználó osztálya")]
		public virtual Class? Class { get; set; }

		/// <summary>
		/// Felhasználó beosztása
		/// </summary>
		[Display(Name = "Felhasználó beosztása")]
		[Required(ErrorMessage = "A beosztás megadása kötelező!")]
		public Nullable<int> PositionId { get; set; }

		[Display(Name = "Felhasználó beosztása")]
		public virtual Position? Position { get; set; }

		/// <summary>
		/// Archiválva
		/// </summary>
		[Display(Name = "Archiválva")]
		public bool IsArchived { get; set; } = false;

		/// <summary>
		/// Felhasználó által igényelt igénylések
		/// </summary>
		[Display(Name = "Felhasználó által igényelt igénylések")]
		[InverseProperty("RequestAuthor")]
		public virtual List<Request>? RequestAuthor { get; set; }

		/// <summary>
		/// Felhasználónak kért igénylések
		/// </summary>
		[Display(Name = "Felhasználónak kért igénylések")]
		[InverseProperty("RequestFor")]
		public virtual List<Request>? RequestFor { get; set; }

		/// <summary>
		/// Felhasználó által elintézett igénylések
		/// </summary>
		[Display(Name = "Felhasználó által elintézett igénylések")]
		[InverseProperty("Resolver")]
		public virtual List<Request>? Resolver { get; set; }

		/// <summary>
		/// Osztályok ahol engedélyező
		/// </summary>
		[Display(Name = "Osztályok ahol engedélyező")]
		[InverseProperty("Authorizer")]
		public virtual List<Class>? Authorizer { get; set; }
	}
}