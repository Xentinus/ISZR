using ISZR.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISZR.Web.ViewModels
{
    public class GroupViewModel
    {
        /// <summary>
        /// Csoport azonosítója
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Csoport neve
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Kiválasztott jogosultságok azonosítói
        /// </summary>
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
        /// <summary>
        /// Kiválasztható jogosultságok azonosítói
        /// </summary>
        public IEnumerable<SelectListItem> PermissionItems { get; set; } = Enumerable.Empty<SelectListItem>();

    }
}
