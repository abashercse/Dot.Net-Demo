using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationReportModel
    {
        public string txtRequestDateFrom { get; set; }
        public string txtRequestDateTo { get; set; }
        public List<ActivationReqMasterModel> list { get; set; }
    }
}
