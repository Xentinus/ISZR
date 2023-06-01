using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Külső telefonáláshoz PIN kód
	/// </summary>
	public class Phone
	{
		/// <summary>
		/// Azonosító
		/// </summary>
		public int PhoneId { get; set; }

		/// <summary>
		/// PIN kód
		/// </summary>
		[Display(Name = "PIN kód")]
		[Required(ErrorMessage = "A PIN kód megadása kötelező!")]
		[MinLength(6, ErrorMessage = "A PIN kód nem lehet kevesebb mint 6 karakter")]
		[MaxLength(6, ErrorMessage = "A PIN kód nem lehet nagyobb mint 6 karakter")]
		public string? PhoneCode { get; set; }

		/// <summary>
		/// Hozzátartozó felhasználó
		/// </summary>
		[Display(Name = "Hozzátartozó felhasználó")]
		public Nullable<int> PhoneUserId { get; set; }

		[Display(Name = "Hozzátartozó felhasználó")]
		[ForeignKey("PhoneUserId")]
		public virtual User? PhoneUser { get; set; }

		/// <summary>
		/// Archiválva
		/// </summary>
		[Display(Name = "Archiválva")]
		public bool IsArchived { get; set; } = false;
	}
}