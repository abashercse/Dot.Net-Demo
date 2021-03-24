using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationDashboardModel
    {
        public string txtExecutionDate { get; set; }
        public List<SelectListItem> executor { get; set; }
        public string selectedExecutor { get; set; }
        public List<ActivationReqMasterModel> listReqMaster { get; set; }
        public List<ActivationTicketSummaryModel> listTicketSummary { get; set; }
        public List<ActivationTeamSummaryModel> listTeamSummary { get; set; }



        public string citylist { get; set; }
        public string villlist { get; set; }
    }
}
