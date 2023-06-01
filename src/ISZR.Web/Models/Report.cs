using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Hibabejelentések
	/// </summary>
	public class Report
	{
		/// <summary>
		/// Hibabejelentés azonosítója
		/// </summary>
		public int ReportId { get; set; }

		/// <summary>
		/// Bejelentő felhasználó
		/// </summary>
		[Display(Name = "Bejelentő felhasználó")]
		public Nullable<int> ReportUserId { get; set; }

		[Display(Name = "Bejelentő felhasználó")]
		[ForeignKey("ReportUserId")]
		public virtual User? ReportUser { get; set; }

		/// <summary>
		/// Bejelentés megnevezése
		/// </summary>
		[Display(Name = "Bejelentés megnevezése")]
		[MinLength(4, ErrorMessage = "A bejelentés megnevezése nem lehet kevesebb mint 4 karakter")]
		[MaxLength(48, ErrorMessage = "A bejelentés megnevezése nem lehet több mint 48 karakter")]
		public string? Title { get; set; }

		/// <summary>
		/// Bejelentés leírása
		/// </summary>
		[Display(Name = "Bejelentés leírása")]
		[MinLength(10, ErrorMessage = "A bejelentés leírása nem lehet kevesebb mint 10 karakter")]
		public string? Description { get; set; }

		/// <summary>
		/// Bejelentés állapota
		/// </summary>
		[Display(Name = "Bejelentés állapota")]
		public bool IsSolved { get; set; } = false;
	}
}