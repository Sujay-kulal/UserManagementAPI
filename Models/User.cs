using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// DataAnnotations are used for automatic model validation by ASP.NET Core.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user. Assigned by the server; ignored on create.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user's full name. Required and cannot be empty or whitespace.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The user's email address. Required and must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}
