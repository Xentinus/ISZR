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
		[Key]
		public int PositionId { get; set; }

		/// <summary>
		/// Beosztás megnevezése
		/// </summary>
		[Display(Name = "Beosztás megnevezése")]
		[Required(ErrorMessage = "A beosztás megnevezése kötelező!")]
		[MinLength(3, ErrorMessage = "A beosztás neve nem lehet kevesebb mint 3 karakter")]
		[MaxLength(48, ErrorMessage = "A beosztás neve nem lehet nagyobb mint 48 karakter")]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Beosztáshoz hozzátartozó felhasználók
		/// </summary>
		[Display(Name = "Beosztáshoz hozzátartozó felhasználók")]
		public virtual List<User>? Users { get; set; }

		/// <summary>
		/// Archiválva
		/// </summary>
		[Display(Name = "Archiválva")]
		public bool IsArchived { get; set; } = false;
	}
}