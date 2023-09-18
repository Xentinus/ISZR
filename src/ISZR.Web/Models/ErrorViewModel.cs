namespace ISZR.Web.Models
{
    public class ErrorViewModel
    {
        /// <summary>
        /// Hiba azonosító
        /// </summary>
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? Message { get; internal set; }
        public int ErrorCode { get; internal set; } = 500;
    }
}