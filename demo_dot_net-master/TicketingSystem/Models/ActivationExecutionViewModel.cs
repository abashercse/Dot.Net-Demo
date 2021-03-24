using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ActivationExecutionViewModel
    {
        public string ticketId { get; set; }
        public List<SelectListItem> executor { get; set; }
        public ExecutionMasterModel executionMaster { get; set; }
        public string selectedExecutor { get; set; }
        public List<MsisdnSimMasterModel> msisdnList { get; set; }
        public List<ExecutionHistory> executionHistories { get; set; }
        public string isMailEditorVisible { get; set; }
        public List<SelectListItem> execType { get; set; }
        public string selectedExecType { get; set; }
        public List<SelectListItem> ticketStatus { get; set; }
        public string selectedTicketStatus { get; set; }
        public string attachmentTable { get; set; }
        
        
        public string txtMailTo { get; set; }
        public string txtMailCC { get; set; }
        public string txtMailSubject { get; set; }
        public string txtMailAttachment { get; set; }
        public string txtMailBody { get; set; }
        public string divMailAttachment { get; set; }
    }

    public enum TicketStatus
    {
        OPEN,
        CLOSE,
        ASSIGNED,
        HOLD,
        REJECTED
    }
}
