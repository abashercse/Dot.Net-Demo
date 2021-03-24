using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationTicketSummaryModel
    {
        public string requestDate { get; set; }
        public string totalReceived { get; set; }
        public string totalAssigned { get; set; }
        public string totalClosed { get; set; }
        public string totalHold { get; set; }
        public string totalRejected { get; set; }
        public string totalPending { get; set; }
    }
}
