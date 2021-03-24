using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketingSystem.Models
{
    public class ChannelManagementModel
    {


      //  public List<SelectListItem> channeltype { get; set; }
        //public List<SelectListItem> channelname { get; set; }

        public string ddlChannelType { get; set; }
        public string ddlChannelName { get; set; }

        public List<SelectListItem> channelType { get; set; }
        public List<SelectListItem> channelName { get; set; }
        public string txtChannelName { get; set; }
     




    }
}