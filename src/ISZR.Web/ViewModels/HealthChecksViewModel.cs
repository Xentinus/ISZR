namespace ISZR.Web.ViewModels
{
    public class HealthChecksViewModel
    {
        /// <summary>
        /// Adatbázis kapcsolat állapota
        /// </summary>
        public bool DatabaseStatus { get; set; } = false;

        /// <summary>
        /// E-mailes értesítési rendszer állapota
        /// </summary>
        public bool EmailServiceStatus { get; set; } = false;

        /// <summary>
        /// Active-Directory rendszer állapota
        /// </summary>
        public bool LDAPServiceStatus { get; set; } = false;
        public bool LDAPConnectionStatus { get; set; } = false;

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
