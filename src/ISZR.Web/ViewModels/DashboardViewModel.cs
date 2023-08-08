using ISZR.Web.Models;

namespace ISZR.Web.ViewModels
{
    public class DashboardViewModel
    {
        /// <summary>
        /// Felhasználó
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Felhasználó parkolási igényei
        /// </summary>
        public List<Parking>? Parkings { get; set; } = new List<Parking>();

        /// <summary>
        /// Felhasználó által használt PIN kódok
        /// </summary>
        public List<Phone>? Phones { get; set; } = new List<Phone>();
    }
}