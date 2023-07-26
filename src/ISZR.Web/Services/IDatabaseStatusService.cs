using Microsoft.Data.SqlClient;

namespace ISZR.Web.Services
{
    /// <summary>
    /// Adatbázis állapotának ellenőrzése (interface)
    /// </summary>
    public interface IDatabaseStatusService
    {
        bool IsDatabaseOnline();
    }

    /// <summary>
    /// Adatbázis állapotának ellenőrzése
    /// </summary>
    public class DatabaseStatusService : IDatabaseStatusService
    {
        private readonly string _connectionString;

        public DatabaseStatusService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DataContext");
        }

        /// <summary>
        /// Elérhető e a megadott adatbázis
        /// </summary>
        public bool IsDatabaseOnline()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
