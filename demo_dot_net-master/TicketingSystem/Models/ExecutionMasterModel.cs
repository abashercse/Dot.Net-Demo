using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ExecutionMasterModel
    {
        public string ticketId { get; set; }
        public string activationTeam { get; set; }
        public string requestedBy { get; set; }
        public string requestedDate { get; set; }
        public string serviceName { get; set; }
        public string productName { get; set; }
        public string channelType { get; set; }
        public string channelName { get; set; }
        public string checkUrgent { get; set; }
        public string totalQuantity { get; set; }
        public string remarks { get; set; }
        public string comments { get; set; }
        public string remainingQuantity { get; set; }
        public string executionQuantity { get; set; }
        public string execType { get; set; }
        public string ticketStatus { get; set; }
        public string execUser { get; set; }
    }
}
