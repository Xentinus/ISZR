namespace ISZR.Models
{
    public class ErrorViewModel
    {
        /// <summary>
        /// Hiba azonosító
        /// </summary>
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}