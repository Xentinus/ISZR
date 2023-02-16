namespace ISZR.Web.Models
{
	public class ErrorViewModel
	{
		/// <summary>
		/// Hiba azonos�t�
		/// </summary>
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}