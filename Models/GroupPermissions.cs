using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Csoportokhoz hozzátartozó jogosultságok
    /// </summary>
    public class GroupPermissions
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Csoport
        /// </summary>
        public Nullable<int> GroupId { get; set; }

        public Group? Group { get; set; }

        /// <summary>
        /// Hozzá tartozó jogosultságk
        /// </summary>
        public Nullable<int> WindowsPermissionId { get; set; }

        public WindowsPermission? WindowsPermission { get; set; }
    }
}