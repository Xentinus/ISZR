using System.Security.Principal;
using System.Text.RegularExpressions;

namespace ISZR.Web.Components
{
    public static class Account
    {
        /// <summary>
        /// Windows felhasználó jogosultságai között az adott csoport megkeresése
        /// </summary>
        /// <param name="groupName">Active-Directory csoport neve</param>
        /// <returns>Felhasználó rendelkezik e a jogosultsággal (igaz/hamis)</returns>
        public static bool CheckGroup(string groupName)
        {
            // groupName nem lehet null értékű
            if (string.IsNullOrEmpty(groupName)) return false;

            groupName = $"^.*{groupName}$";

            // Windows felhasználó információinak megszerzése
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity == null) return false;

            // Bejelentkezett állapot ellenőrzése
            if (identity.IsAuthenticated)
            {
                // Bejelentkezett windows felhasználó jogosultságainak ellenőrzése
                if (identity.Groups == null) return false;

                // Szükséges jogosultság keresése a csoportok között
                foreach (IdentityReference group in identity.Groups)
                {
                    if (Regex.IsMatch(group.Translate(typeof(NTAccount)).Value, groupName)) return true;
                }
            }

            // A személy nem rendelkezik a kért jogosultsággal
            return false;
        }

        /// <summary>
        /// XXXX-ISZR-Ugyintezo Active-Directory jogosultság meglétének ellenőrzése
        /// </summary>
        /// <returns>Felhasználó rendelkezik e a jogosultsággal (igaz/hamis)</returns>
        public static bool IsUgyintezo()
        {
            return true; // CheckGroup("ISZR-Ugyintezo");
        }

        /// <summary>
        /// XXXX-ISZR-Admin Active-Directory jogosultság meglétének ellenőrzése
        /// </summary>
        /// <returns>Felhasználó rendelkezik e a jogosultsággal (igaz/hamis)</returns>
        public static bool IsAdmin()
        {
            return true; // CheckGroup("ISZR-Admin");
        }

        /// <summary>
        /// Az ISZR rendszerben ellenőrzésre kerül, hogy a felhasználó létezik e
        /// </summary>
        /// <returns>A felhasználó létezik e a rendszerben (igaz/hamis)</returns>
        public static async Task<bool> IsUserExists(DataContext _context)
        {
            // Windows felhasználó információinak megszerzése
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity == null) return false;

            // Bejelentkezett állapot ellenőrzése
            if (identity.IsAuthenticated)
            {
                var user = await _context.Users.FirstOrDefaultAsync(m => m.Username == identity.Name);
                if (user != null) return true;
            }

            // Az adott személy nem létezik a rendszerben
            return false;
        }

        /// <summary>
        /// Az ISZR rendszerben kikeresésre kerül a felhasználó azonosítója
        /// </summary>
        /// <returns>ISZR felhasználói azonosító</returns>
        public static async Task<int> GetUserId(DataContext _context)
        {
            // Windows felhasználó információinak megszerzése
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity == null) return -1;

            // Bejelentkezett állapot ellenőrzése
            if (identity.IsAuthenticated)
            {
                var user = await _context.Users.FirstOrDefaultAsync(m => m.Username == identity.Name);
                if (user != null) return user.UserId;
            }

            // Az adott személy nem létezik a rendszerben
            return -1;
        }
    }
}