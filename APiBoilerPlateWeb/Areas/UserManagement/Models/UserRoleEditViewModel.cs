using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APiBoilerPlateWeb.Areas.UserManagement.Models
{
    public class UserRoleEditViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<UserRolesViewModel> UserRoles { get; set; }
    }
}
