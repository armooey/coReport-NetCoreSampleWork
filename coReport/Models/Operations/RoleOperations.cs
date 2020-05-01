using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Operations
{
    public static class RoleOperations
    {
        public static SelectList GetRolesSelectList(this RoleManager<IdentityRole<short>> roleManager)
        {
            return new SelectList(roleManager.Roles.Where(r => r.Name != "ادمین").Select(r => r.Name).ToList());
        }
    }
}
