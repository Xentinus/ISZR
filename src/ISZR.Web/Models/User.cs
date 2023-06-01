using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Nemek
	/// </summary>
	public enum Genre
	{
		Male,
		Female
	}

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
		[MinLength(3, ErrorMessage = "A megjeleníthető név nem lehet kevesebb mint 3 karakter")]
		[MaxLength(64, ErrorMessage = "A megjeleníthető név nem lehet nagyobb mint 64 karakter")]
		public string? DisplayName { get; set; }

		/// <summary>
		/// Rendfokozat
		/// </summary>
		[Display(Name = "Rendfokozat")]
		[Required(ErrorMessage = "A rendfokozat kiválasztása kötelező!")]
		public string? Rank { get; set; }

		/// <summary>
		/// Megszólítás
		/// </summary>
		[Display(Name = "Megszólítás")]
		[Required(ErrorMessage = "A megszólítás kiválasztása kötelező!")]
		public Genre Genre { get; set; } = Genre.Male;

		/// <summary>
		/// Felhasználó emailes elérhetősége
		/// </summary>
		[Display(Name = "E-mail cím")]
		[EmailAddress(ErrorMessage = "A mezőbe beírt cím nem e-mail cím!")]
		public string? Email { get; set; }

		/// <summary>
		/// Felhasználó telefonos elérhetősége
		/// </summary>
		[Display(Name = "NTG elérhetőség")]
		[MaxLength(7, ErrorMessage = "Az NTG elérhetőség nem lehet nagyobb mint 7 karakter")]
		public string? Phone { get; set; }

		/// <summary>
		/// Felhasználó szolgálati helye
		/// </summary>
		[Display(Name = "Szolgálati hely")]
		[Required(ErrorMessage = "A szolgálati hely megadása kötelező!")]
		[MaxLength(64, ErrorMessage = "A szolgálati hely megnevezése nem lehet 64 karakternél hosszabb")]
		public string? Location { get; set; }

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
		[InverseProperty("CreatedByUser")]
		public virtual List<Request>? CreatedByUser { get; set; }

		/// <summary>
		/// Felhasználónak kért igénylések
		/// </summary>
		[Display(Name = "Felhasználónak kért igénylések")]
		[InverseProperty("CreatedForUser")]
		public virtual List<Request>? CreatedForUser { get; set; }

		/// <summary>
		/// Felhasználó által elintézett igénylések
		/// </summary>
		[Display(Name = "Felhasználó által elintézett igénylések")]
		[InverseProperty("ClosedByUser")]
		public virtual List<Request>? ClosedByUser { get; set; }

		/// <summary>
		/// Felhasználóhoz hozzátartozó járművek
		/// </summary>
		[Display(Name = "Felhasználóhoz hozzátartozó járművek")]
		[InverseProperty("OwnerUser")]
		public virtual List<Parking>? Cars { get; set; }

		/// <summary>
		/// Felhasználóhoz hozzátartozó hibabejelentések
		/// </summary>
		[Display(Name = "Felhasználóhoz hozzátartozó hibabejelentések")]
		[InverseProperty("ReportUser")]
		public virtual List<Report>? Reports { get; set; }

		/// <summary>
		/// Felhasználóhoz hozzátartozó PIN kódok
		/// </summary>
		[Display(Name = "Felhasználóhoz hozzátartozó PIN kódok")]
		[InverseProperty("PhoneUser")]
		public virtual List<Phone>? Phones { get; set; }

		/// <summary>
		/// Osztályok ahol engedélyező
		/// </summary>
		[Display(Name = "Osztályok ahol engedélyező")]
		[InverseProperty("Authorizer")]
		public virtual List<Class>? Authorizer { get; set; }
	}
}