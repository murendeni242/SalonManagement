using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.DTOs
{
    /// <summary>
    /// Login response returned to the client.
    /// Contains everything the frontend needs immediately after login
    /// so it does not have to decode the JWT manually.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Database primary key of the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Signed JWT to send as Authorization: Bearer {token} on subsequent requests.</summary>
        public string Token { get; set; } = default!;

        /// <summary>
        /// The user's system role: Owner | Reception | Staff.
        /// Frontend uses this to show/hide nav items and protect routes.
        /// </summary>
        public string Role { get; set; } = default!;

        /// <summary>Login email — useful to display "Logged in as owner@salon.com" in the UI.</summary>
        public string Email { get; set; } = default!;

        /// <summary>UTC timestamp when this token expires. Frontend can use this to auto-logout.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Current status of the account: Active | Inactive.
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// True when the account was created by an Owner and the user has not yet chosen their own password.
        /// </summary>
        public bool MustChangePassword { get; set; }

        /// <summary>
        /// UTC timestamp when the account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// UTC timestamp of the user's last login. Null if the user has never logged in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
