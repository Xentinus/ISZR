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
        public int AllRequests { get; set; }

        /// <summary>
        /// Felhasználó által kért igénylések amelyek végre lettek hajtva
        /// </summary>
        public int DoneRequests { get; set; }

        /// <summary>
        /// Felhasználó által kért igénylések amelyek még folyamatban vannak
        /// </summary>
        public int InProgressRequests { get; set; }

        /// <summary>
        /// Felhasználó által kért igénylések amelyek el lettek utasítva
        /// </summary>
        public int DeniedRequests { get; set; }

        /// <summary>
        /// Felhasználó parkolási igényei
        /// </summary>
        public List<Parking>? Parkings { get; set; }
        public DashboardViewModel()
        {
            Parkings = new List<Parking>();
        }
    }
}