using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace Domain.Helpers
{
    public static class Roles
    {
        public const string SaasAdmin = nameof(UserRole.SaasAdmin);         // "SaasAdmin"
        public const string BusinessOwner = nameof(UserRole.BusinessOwner); // "BusinessOwner"
        public const string Manager = nameof(UserRole.Manager);             // "Manager"
        public const string Staff = nameof(UserRole.Staff);                 // "Staff"

        public static readonly string[] All = { SaasAdmin, BusinessOwner, Manager, Staff };
    }
}
