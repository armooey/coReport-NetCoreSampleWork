using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Data
{
    public static class AppSettingInMemoryDatabase
    {
        private static String _adminUsername;
        private static String _adminPassword;
        private static String _adminRoleName;
        private static String _managerRoleName;
        private static String _employeeRoleName;
        public static IList<String> IMAGE_FORMATS;
        public static string ADMIN_USERNAME { get => _adminUsername; }
        public static string ADMIN_PASSWORD { get => _adminPassword; }
        public static string ADMIN_ROLE_NAME { get => _adminRoleName; }
        public static string MANAGER_ROLE_NAME { get => _managerRoleName; }
        public static string EMPLOYEE_ROLE_NAME { get => _employeeRoleName; }

        public static void Initialize(IConfiguration configuration)
        {
            _adminUsername = configuration.GetSection("AdminAuthentication").GetValue<String>("Username");
            _adminPassword = configuration.GetSection("AdminAuthentication").GetValue<String>("Password");
            _adminRoleName = configuration.GetSection("SystemRoleNames").GetValue<String>("Admin");
            _managerRoleName = configuration.GetSection("SystemRoleNames").GetValue<String>("Manager");
            _employeeRoleName = configuration.GetSection("SystemRoleNames").GetValue<String>("Employee");
            IMAGE_FORMATS = configuration.GetSection("ImageFormats").Get<List<String>>().AsReadOnly();
        }
    }
}
