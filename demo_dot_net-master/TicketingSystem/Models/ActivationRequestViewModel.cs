using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationRequestViewModel
    {
        public string ticketId { get; set; }
        public List<SelectListItem> ddlActivationTeam { get; set; }
        public string selectedActivationTeam { get; set; }
        public List<SelectListItem> ddlService { get; set; }
        public string selectedService { get; set; }
        public List<SelectListItem> ddlProduct { get; set; }
        public string selectedProduct { get; set; }
        public List<SelectListItem> ddlChannelType { get; set; }
        public string selectedChannel { get; set; }
        public List<SelectListItem> ddlChannelName { get; set; }
        public string selectedChannelName { get; set; }
        public List<MsisdnSimMasterModel> msisdnSims { get; set; }
        public string attachmentTable { get; set; }

        public string txtMailTo { get; set; }
        public string txtMailCC { get; set; }
        public string txtMailSubject { get; set; }
        public string txtMailAttachment { get; set; }
        public string txtMailBody { get; set; }
        public string divMailAttachment { get; set; }
    }

}
