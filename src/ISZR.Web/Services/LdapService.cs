using ISZR.Web.Models;
using Novell.Directory.Ldap;
using System.Collections;

namespace ISZR.Web.Services
{
    public class LdapService
    {
        private readonly bool ldapEnable;
        private readonly string? ldapServer;
        private readonly string? ldapBindUser;
        private readonly string? ldapBindPassword;
        private readonly string? ldapBaseDN;

        public LdapService(IConfiguration configuration)
        {
            ldapEnable = configuration.GetValue<bool>("ApplicationSettings:LDAP_ENABLE");
            ldapServer = configuration.GetValue<string>("ApplicationSettings:LDAP_SERVER");
            ldapBindUser = configuration.GetValue<string>("ApplicationSettings:LDAP_BIND_USER");
            ldapBindPassword = configuration.GetValue<string>("ApplicationSettings:LDAP_BIND_PASSWORD");
            ldapBaseDN = configuration.GetValue<string>("ApplicationSettings:LDAP_BASE");
        }

        public bool IsLdapConnectionSuccessful()
        {
            // LDAP szolgáltatás bekapcsolásának ellenőrzése
            if (!ldapEnable) return false;

            try
            {
                // LDAP csatlakozás beállítása
                LdapConnection conn = new LdapConnection();
                conn.Connect(ldapServer, 389);

                // Csatlakozás a felhasználóval
                conn.Bind(ldapBindUser, ldapBindPassword);

                // Lecsatlakozás
                conn.Disconnect();

                // Sikeres eredmény továbbítása
                return true;
            }
            catch (Exception ex)
            {
                // Hiba leírása
                Console.WriteLine(ex.Message.ToString());

                // Sikertelen eredmény továbbítása
                return false;
            }
        }

        public void UpdateUser(User user, string ClassName, string PositionName)
        {
            // LDAP szolgáltatás bekapcsolásának ellenőrzése
            if (!ldapEnable) return;

            // Felhasználónév nélküli emberek frissítésének kihagyása
            if (string.IsNullOrEmpty(user.Username)) return;

            try
            {
                // Módosításra váró elemek összeállítása
                ArrayList modList = new ArrayList();

                // Megjelenítési név módosítása
                LdapAttribute displayName = new LdapAttribute("displayName", user.DisplayName);
                modList.Add(new LdapModification(LdapModification.Replace, displayName));

                // Leírás módosítása
                LdapAttribute description = new LdapAttribute("description", $"{ClassName} - {PositionName}");
                modList.Add(new LdapModification(LdapModification.Replace, description));

                // Helyszín módosítása
                LdapAttribute office = new LdapAttribute("physicalDeliveryOfficeName", user.Location);
                modList.Add(new LdapModification(LdapModification.Replace, office));

                // NTG Elérhetőség módosítása
                LdapAttribute phone = new LdapAttribute("telephoneNumber", user.Phone);
                modList.Add(new LdapModification(LdapModification.Replace, phone));

                // Pozició módosítása
                LdapAttribute title = new LdapAttribute("title", PositionName);
                modList.Add(new LdapModification(LdapModification.Replace, title));

                // Osztály/Csoport módosítása
                LdapAttribute department = new LdapAttribute("department", ClassName);
                modList.Add(new LdapModification(LdapModification.Replace, department));

                // Elemek összeállítása
                LdapModification[] mods = new LdapModification[modList.Count];
                mods = (LdapModification[])modList.ToArray(typeof(LdapModification));

                // LDAP csatlakozás beállítása
                LdapConnection conn = new LdapConnection();
                conn.Connect(ldapServer, 389);

                // Felcsatlakozás a felhasználóval
                conn.Bind(ldapBindUser, ldapBindPassword);

                // Felhasználó DN lekérdezésének összeállítása
                string[] parts = user.Username.Split('\\');
                var filter = $"(&(objectClass=user)(sAMAccountName={parts[1]}))";

                // Felhasználó megkeresése az Active-Directory-ban
                LdapSearchResults lookingUser = (LdapSearchResults)conn.Search(ldapBaseDN,
                                                LdapConnection.ScopeSub,
                                                filter,
                                                null,
                                                false);

                // Felhasználó kiválasztása
                var entry = lookingUser.FirstOrDefault();

                // Felhasználó létezésének ellenőrzése
                if (entry != null)
                {
                    // Felhasználó értékeinek módosítása
                    conn.Modify(entry.Dn, mods);
                }

                // Lecsatlakozás
                conn.Disconnect();
            }
            catch (LdapException e)
            {
                Console.WriteLine("Error:" + e.LdapErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e.Message);
            }
        }
    }
}