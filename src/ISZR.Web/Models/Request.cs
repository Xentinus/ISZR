using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISZR.Web.Models
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
		public string? Type { get; set; }

		/// <summary>
		/// Igénylés státusza
		/// </summary>
		[Display(Name = "Igénylés státusza")]
		public string? Status { get; set; }

		/// <summary>
		/// Igénylés leírása
		/// </summary>
		[Display(Name = "Igénylés leírása")]
		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		/// <summary>
		/// Igényelt Windows jogosultságok
		/// </summary>
		[Display(Name = "Windows jogosultságok")]
		[DataType(DataType.MultilineText)]
		public string? WindowsPermissions { get; set; }

		/// <summary>
		/// Igényelt Főnix 3 jogosultságok
		/// </summary>
		[Display(Name = "Főnix 3 jogosultságok")]
		[DataType(DataType.MultilineText)]
		public string? FonixPermissions { get; set; }

		/// <summary>
		/// Igényelt autó
		/// </summary>
		[Display(Name = "Igényelt autó")]
		public Nullable<int> CarId { get; set; }

		[Display(Name = "Igényelt autó")]
		[ForeignKey("CarId")]
		public virtual Parking? Car { get; set; }

		/// <summary>
		/// Igényelt NTG kód
		/// </summary>
		[Display(Name = "Igényelt NTG kód")]
		public Nullable<int> PhoneId { get; set; }

		[Display(Name = "Igényelt NTG kód")]
		[ForeignKey("PhoneId")]
		public virtual Phone? Phone { get; set; }

		/// <summary>
		/// Igénylés készítésének időpontja
		/// </summary>
		[Display(Name = "Igénylés készítésének időpontja")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. HH:mm}")]
		public DateTime CreatedDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// Igénylést intéző felhasználó
		/// </summary>
		[Display(Name = "Igénylést intéző felhasználó")]
		public Nullable<int> CreatedByUserId { get; set; }

		[Display(Name = "Igénylést intéző felhasználó")]
		[ForeignKey("CreatedByUserId")]
		public virtual User? CreatedByUser { get; set; }

		/// <summary>
		/// Személy aki számára az igénylés zajlik
		/// </summary>
		[Display(Name = "Személy aki számára az igénylés zajlik")]
		public Nullable<int> CreatedForUserId { get; set; }

		[Display(Name = "Személy aki számára az igénylés zajlik")]
		[ForeignKey("CreatedForUserId")]
		public virtual User? CreatedForUser { get; set; }

		/// <summary>
		/// Igénylés elintézésének ideje
		/// </summary>
		[Display(Name = "Igénylés elintézésének ideje")]
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy. MMMM dd. HH:mm}")]
		public DateTime ClosedDateTime { get; set; }

		/// <summary>
		/// Személy aki lezárja az igénylést
		/// </summary>
		[Display(Name = "Személy aki lezárja az igénylést")]
		public Nullable<int> ClosedByUserId { get; set; }

		[Display(Name = "Személy aki lezárja az igénylést")]
		[ForeignKey("ClosedByUserId")]
		public virtual User? ClosedByUser { get; set; }
	}
}