using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class SearchMyTicketModel
    {
        public string txtRequestDateFrom { get; set; }
        public string txtRequestDateTo { get; set; }
        public string txtTicketNo { get; set; }
        public string ddlTicketStatus { get; set; }
        public List<SelectListItem> ticketStatus { get; set; }
        public List<ActivationReqMasterModel> reqMstList { get; set; }
    }
}