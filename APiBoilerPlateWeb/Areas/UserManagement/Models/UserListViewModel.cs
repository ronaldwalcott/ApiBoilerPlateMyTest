using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APiBoilerPlateWeb.Areas.UserManagement.Models
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string RoleNames { get; set; }
        public Boolean LockoutEnabled { get; set; }
        public string EmployeeNumber { get; set; }
        public List<UserRolesViewModel> UserRoles { get; set; }
    }
}
