using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationTeamSummaryModel
    {
        public string userId { get; set; }
        public string totalAssigned { get; set; }
        public string totalClosed { get; set; }
        public string totalHold { get; set; }
        public string  totalRejected { get; set; }
    }
}
