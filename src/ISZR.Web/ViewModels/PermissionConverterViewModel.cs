using ISZR.Web.Models;

namespace ISZR.Web.ViewModels
{
    public class PermissionConverterViewModel
    {
        public string UserInput { get; set; } = string.Empty;
        public HashSet<Permission> FoundPermissions { get; set; } = new HashSet<Permission>();
        public List<string> NotFoundPermissions { get; set; } = new List<string>();

    }
}
