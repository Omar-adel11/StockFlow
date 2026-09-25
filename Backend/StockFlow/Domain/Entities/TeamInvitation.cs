using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TeamInvitation
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        // Only ever "Manager" or "Staff" - enforced in the service layer,
        // never trusted as free text from the request.
        public string Role { get; set; } = string.Empty;

        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public int InvitedByUserId { get; set; }

        // Long, unguessable, unique - this is effectively a bearer
        // credential for creating an account inside your business.
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }

        // Nullable timestamp instead of a bool - tells you both "was this
        // used" and "when", which a bare IsUsed = true would lose.
        public DateTime? AcceptedAtUtc { get; set; }

        // Lets a Business Owner cancel a pending invite before it's used -
        // e.g. they invited the wrong email by mistake.
        public DateTime? RevokedAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Convenience - an invite is usable only if none of these apply.
        public bool IsActive =>
            AcceptedAtUtc is null && RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
    }
}
