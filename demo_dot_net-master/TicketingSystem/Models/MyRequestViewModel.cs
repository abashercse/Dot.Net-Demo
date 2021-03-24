using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class MyRequestViewModel
    {
        public RequestLinkMasterModel linkCaptions { get; set; }
        public List<RequestDetailsMaster> listDetails { get; set; }
    }
}
