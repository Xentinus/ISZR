using System.Security.Principal;
using System.Text.RegularExpressions;

namespace ISZR.Components
{
	public static class Security
	{
		public static bool CheckGroup(string groupName)
		{
			// groupName nem lehet null értékű
			if (groupName == null) return false;

			groupName = $"^.*{groupName}.*$";

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
		/// XXXX-ISZR-Ugyintezo Active-Directory jogosultságának meglétének ellenőrzése
		/// </summary>
		public static bool IsUgyintezo()
		{
			return CheckGroup("ISZR-Ugyintezo");
		}

		/// <summary>
		/// XXXX-ISZR-Admin Active-Directory jogosultságának
		/// </summary>
		public static bool IsAdmin()
		{
			// return checkgroup
			return CheckGroup("ISZR-Admin");
		}
	}
}