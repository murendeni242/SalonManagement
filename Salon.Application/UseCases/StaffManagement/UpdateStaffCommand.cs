namespace Salon.Application.UseCases.StaffManagement
{
    /// <summary>
    /// Input for updating an existing staff profile. Owner role only.
    /// Soft-deleted staff cannot be updated.
    /// </summary>
    public class UpdateStaffCommand
    {
        /// <summary>Primary key of the staff to update. Set from the URL route parameter.</summary>
        public int Id { get; set; }

        /// <summary>New first name. Required.</summary>
        public string FirstName { get; set; } = default!;

        /// <summary>New last name. Required.</summary>
        public string LastName { get; set; } = default!;

        /// <summary>New contact phone number.</summary>
        public string Phone { get; set; } = default!;

        /// <summary>New salon role. Required.</summary>
        public string Role { get; set; } = default!;

        /// <summary>New status. Must be "Active" or "Inactive".</summary>
        public string Status { get; set; } = "Active";

        /// <summary>New email, or null to clear.</summary>
        public string? Email { get; set; }

        /// <summary>
        /// Full replacement of specialisations.
        /// Pass an empty list to remove all restrictions (staff can do all services).
        /// </summary>
        public List<int> Specialisations { get; set; } = new();
    }
}
