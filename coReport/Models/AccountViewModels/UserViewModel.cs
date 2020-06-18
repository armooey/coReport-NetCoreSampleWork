using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AccountViewModels
{
    public class UserViewModel
    {
        public short Id { get; set; }
        public String Username { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public bool HasImage { get; set; }
        public String Role { get; set; }
        public String RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
