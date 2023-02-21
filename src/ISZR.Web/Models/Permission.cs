using System.ComponentModel.DataAnnotations;

namespace ISZR.Web.Models
{
	/// <summary>
	/// Jogosultság
	/// </summary>
	public class Permission
	{
		/// <summary>
		/// Jogosultság azonosítója
		/// </summary>
		public int PermissionId { get; set; }

		/// <summary>
		/// Jogosultság neve
		/// </summary>
		[Display(Name = "Jogosultság megnevezése")]
		[DataType(DataType.Text)]
		[Required(ErrorMessage = "A jogosultság megnevezése kötelező!")]
		[MinLength(4, ErrorMessage = "A jogosultság neve nem lehet kevesebb mint 4 karakter")]
		[MaxLength(64, ErrorMessage = "A jogosultság neve nem lehet nagyobb mint 64 karakter")]
		public string? Name { get; set; }

        /// <summary>
        /// Jogosultság típusa
        /// </summary>
        [Display(Name = "Jogosultság típusa")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "A típus kiválasztása kötelező!")]
        public string? Type { get; set; }

        /// <summary>
        /// Jogosultság leírása
        /// </summary>
        [Display(Name = "Jogosultság leírása")]
		[DataType(DataType.MultilineText)]
		[Required(ErrorMessage = "A jogosultság leírásának megadása kötelező!")]
		[MinLength(5, ErrorMessage = "A jogosultság leírásának legalább 5 karakter hosszúnak kell lennie!")]
		public string? Description { get; set; }

		/// <summary>
		/// Active Directory jogosultságok
		/// </summary>
		[Display(Name = "Active Directory jogosultságok")]
		[DataType(DataType.MultilineText)]
		public string? ActiveDirectoryPermissions { get; set; }

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}