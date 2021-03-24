using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class UserManagementViewModel
    {
        public string selectedUser { get; set; }
        public List<SelectListItem> userList { get; set; }
        public string selectedDh { get; set; }
        public List<SelectListItem> dhList { get; set; }
        public UserDetailsMasterModel userDetails { get; set; }
    }
}
