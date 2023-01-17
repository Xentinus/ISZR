﻿using System.ComponentModel.DataAnnotations;

namespace ISZR.Models
{
    /// <summary>
    /// Jogosultság
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Jogosultság azonosítója
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Jogosultság típusa
        /// </summary>
        [Display(Name = "Jogosultság típusa")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "A típus kiválasztása kötelező!")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Jogosultság intézete
        /// </summary>
        [Display(Name = "Intézet")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Az intézet kiválasztása kötelező!")]
        public string Institute { get; set; } = string.Empty;

        /// <summary>
        /// Jogosultság neve
        /// </summary>
        [Display(Name = "Jogosultság megnevezése")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "A jogosultság megnevezése kötelező!")]
        [MinLength(4, ErrorMessage = "A jogosultság neve nem lehet kevesebb mint 4 karakter")]
        [MaxLength(64, ErrorMessage = "A jogosultság neve nem lehet nagyobb mint 64 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Jogosultság leírása
        /// </summary>
        [Display(Name = "Jogosultság leírása")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "A jogosultság leírásának megadása kötelező!")]
        [MinLength(5, ErrorMessage = "A jogosultság leírásának legalább 5 karakter hosszúnak kell lennie!")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Active Directory jogosultságok
        /// </summary>
        [Display(Name = "Active Directory jogosultságok")]
        [DataType(DataType.MultilineText)]
        [MinLength(6, ErrorMessage = "Legalább egy darab jogosultságot írj be! Előtag szükséges!")]
        public string ActiveDirectoryPermissions { get; set; } = string.Empty;

        /// <summary>
        /// Archiválva
        /// </summary>
        [Display(Name = "Archiválva")]
        public bool IsArchived { get; set; } = false;
    }
}