namespace ISZR.Web.ViewModels
{
    public class HealthChecksViewModel
    {
        /// <summary>
        /// Adatbázis kapcsolat állapota
        /// </summary>
        public bool DatabaseStatus { get; set; } = false;

        /// <summary>
        /// Bejelentkezett felhasználók statisztikája
        /// </summary>
        public int LoggedUserToday { get; set; } = 0;

        /// <summary>
        /// Igénylés statisztika
        /// </summary>
        public int RequestAll { get; set; } = 0;
        public int RequestAllDone { get; set; } = 0;
        public int RequestAllProgress { get; set; } = 0;
        public int RequestAllDenied { get; set; } = 0;
        public int RequestClosedToday { get; set; } = 0;
        public int RequestClosedMonth { get; set; } = 0;
        public int RequestOpenToday { get; set; } = 0;
        public int RequestOpenMonth { get; set; } = 0;

        /// <summary>
        /// Adat statisztika
        /// </summary>
        public int ActiveUsers { get; set; } = 0;
        public int ActiveParkings { get; set; } = 0;
        public int ActiveGroups { get; set; } = 0;
        public int ActiveWindowsPermission { get; set; } = 0;
        public int ActiveFonixPermission { get; set; } = 0;
        public int ActivePhones { get; set; } = 0;
        public int ActiveNotUsedPhone { get; set; } = 0;
    }
}
