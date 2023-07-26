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
        /// Felhasználó által kért igénylések összesen
        /// </summary>
        public int AllRequests { get; set; } = 0;

        /// <summary>
        /// Felhasználó által kért igénylések amelyek végre lettek hajtva
        /// </summary>
        public int DoneRequests { get; set; } = 0;

        /// <summary>
        /// Felhasználó által kért igénylések amelyek még folyamatban vannak
        /// </summary>
        public int InProgressRequests { get; set; } = 0;

        /// <summary>
        /// Felhasználó által kért igénylések amelyek el lettek utasítva
        /// </summary>
        public int DeniedRequests { get; set; } = 0;

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