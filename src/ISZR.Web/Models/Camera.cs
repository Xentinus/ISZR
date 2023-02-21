using System.ComponentModel.DataAnnotations;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Intézeti kamera
	/// </summary>
	public class Camera
	{
		/// <summary>
		/// Kamera azonosítója
		/// </summary>
		[Key]
		public int CameraId { get; set; }

		/// <summary>
		/// Kamera megnevezése
		/// </summary>
		[Display(Name = "Kamera megnevezése")]
		[Required(ErrorMessage = "A kamera nevének megadása kötelező!")]
		[MinLength(2, ErrorMessage = "A kamera neve nem lehet kevesebb mint 2 karakter")]
		[MaxLength(64, ErrorMessage = "A kamera neve nem lehet nagyobb mint 64 karakter")]
		public string? Name { get; set; }

		/// <summary>
		/// Kamera helyszíne
		/// </summary>
		[Display(Name = "Kamera helyszíne")]
		[Required(ErrorMessage = "A kamera helyszínének megadása kötelező!")]
		[MinLength(2, ErrorMessage = "A kamera helyszíne nem lehet kevesebb mint 2 karakter")]
		[MaxLength(64, ErrorMessage = "A kamera helyszíne nem lehet nagyobb mint 64 karakter")]
		public string? Location { get; set; }

		/// <summary>
		/// Archiválva
		/// </summary>
		[Display(Name = "Archiválva")]
		public bool IsArchived { get; set; } = false;
	}
}