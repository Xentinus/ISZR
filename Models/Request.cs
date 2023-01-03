using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ISZR.Models
{
	/// <summary>
	/// Az igénylés státuszai
	/// </summary>
	public enum requestStatus
	{
		Végrehajtva,
		Elutasítva,
		Folyamatban
	}

	/// <summary>
	/// Igénylések
	/// </summary>
	public class Request
	{
		/// <summary>
		/// Az igénylés azonosító száma
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Igénylő felhasználói azonosítója
		/// </summary>
		public int? AuthorId { get; set; }

		/// <summary>
		/// Az osztály azonosítója ahova kérvényezve lett az igénylés
		/// </summary>
		public int? ClassId { get; set; }

		/// <summary>
		/// Az igénylés típusa
		/// </summary>
		public string? Type { get; set; }

		/// <summary>
		/// Típuson bellüli típus (pl új felhasználó, többletjogosultság)
		/// </summary>
		public string? TypeOfType { get; set; }

		/// <summary>
		/// Személyek vagy csoportok akiknek az igénylés zajlik
		/// </summary>
		public string? ToWho { get; set; }

		/// <summary>
		/// Az igénylés leírása
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Az igénylés feladásának ideje
		/// </summary>
		public DateTime StartedDate { get; set; } = DateTime.Now;

		/// <summary>
		/// Az igénylés befejezézésnek időpontja
		/// </summary>
		public DateTime FinishedDate { get; set; }

		/// <summary>
		/// Az igénylés státusza
		/// </summary>
		public requestStatus Status { get; set; } = (requestStatus)2;

		/// <summary>
		/// Amennyiben az igénylés el lett utasíva, annak magyarázata
		/// </summary>
		public string? StatusDesc { get; set; }

		/// <summary>
		/// Igénylést elvégző személy azonosítója
		/// </summary>
		public string? StatusAuthorId { get; set; }
	}
}