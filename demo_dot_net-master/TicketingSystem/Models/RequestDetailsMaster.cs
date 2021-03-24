using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class RequestDetailsMaster
    {
        public string requestDate { get; set; }
        public string ticketId { get; set; }
        public string serviceName { get; set; }
        public string status { get; set; }
        public string responsibleTeam { get; set; }
        public string ticketAssignedUser { get; set; }
        public string comments { get; set; }
    }
}
