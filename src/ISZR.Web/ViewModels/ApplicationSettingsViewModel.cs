namespace ISZR.Web.ViewModels
{
    public class ApplicationSettingsViewModel
    {
        /// <summary>
        /// Mailes értesítés bekapcsolása
        /// </summary>
        public bool SNMP_ENABLE { get; set; } = false;

        /// <summary>
        /// Mail szerver elérési útvonala
        /// </summary>
        public string? SNMP_SERVER { get; set; }

        /// <summary>
        /// Küldő e-mail címe
        /// </summary>
        public string? SNMP_MAIL { get; set; }

        /// <summary>
        /// Küldő felhasználó neve (domainel)
        /// </summary>
        public string? SNMP_USERNAME { get; set; }

        /// <summary>
        /// Küldő jelszava
        /// </summary>
        public string? SNMP_PASSWORD { get; set; }

        /// <summary>
        /// Active Directoryval való szinkronizálás
        /// </summary>
        public bool LDAP_ENABLE { get; set; } = false;

        /// <summary>
        /// Active Directory szerver elérési útvonala
        /// </summary>
        public string? LDAP_SERVER { get; set; }

        /// <summary>
        /// Active Directory admin felhasználóneve
        /// </summary>
        public string? LDAP_USERNAME { get; set; }

        /// <summary>
        /// Active Directory admin jelszava
        /// </summary>
        public string? LDAP_PASSWORD { get; set; }

        /// <summary>
        /// LDAP alapértelmezett helyszíne
        /// </summary>
        public string? LDAP_BASE { get; set; }
    }
}
