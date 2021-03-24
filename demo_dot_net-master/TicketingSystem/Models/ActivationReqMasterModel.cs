using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationReqMasterModel
    {
        public string ticketCreateDate { get; set; }
        public string ticketNo { get; set; }
        public string serviceName { get; set; }
        public string productName { get; set; }
        public string isUrgent { get; set; }
        public string ticketCreateUser { get; set; }
        public string ticketStatus { get; set; }
        public string ticketAssignUser { get; set; }
        public string alarm { get; set; }
        public string activationTeam { get; set; }
        public string comments { get; set; }
        public string channelType { get; set; }
        public string channelName { get; set; }
        public string ticketAssignDate { get; set; }
        public string ticketCloseDate { get; set; }
        public string totalExecutionTime { get; set; }
        public string totalWaitingTime { get; set; }
        public string totalQuantity { get; set; }
        public string execQuantity { get; set; }
        public string execUser { get; set; }
    }
}
