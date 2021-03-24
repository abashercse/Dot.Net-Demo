using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class UserModel
    {
        public string txtUserName { get; set; }
        public string txtUserId { get; set; }
        public string txtUserGroup { get; set; }
        public string txtUserType { get; set; }
        public string txtPassword { get; set; }
        public string txtNewPassword { get; set; }
        public string txtConfirmNewPass { get; set; }
        public int count { get; set; }
        public string isLocked { get; set; }

        public string pwResetFlag { get; set; }
        public string pwResetDate { get; set; }
        public string pwSalt { get; set; }

    }
}
