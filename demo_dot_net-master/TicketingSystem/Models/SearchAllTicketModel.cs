using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class SearchAllTicketModel
    {
        public string txtRequestDateFrom { get; set; }
        public string txtRequestDateTo { get; set; }
        public string ddlExecutor { get; set; }
        public List<SelectListItem> executor { get; set; }
        public string ddlProduct { get; set; }
        public List<SelectListItem> product { get; set; }
        public string ddlChannelType { get; set; }
        public List<SelectListItem> channel { get; set; }
        public string ddlService { get; set; }
        public List<SelectListItem> service { get; set; }               
        public string ddlTicketStatus { get; set; }
        public List<SelectListItem> ticketStatus { get; set; }
        public string ddlUrgent { get; set; }
        public List<SelectListItem> urgent { get; set; }
        public string txtTicketNo { get; set; }
        public List<ActivationReqMasterModel> listReqMaster { get; set; }
    }
}
