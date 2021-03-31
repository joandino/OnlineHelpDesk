using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Models.ViewModels
{
    public class TicketViewModel
    {
        public Ticket Ticket { get; set; }
        public SelectList Categories { get; set; }
        public SelectList Statuses { get; set; }
        public SelectList Periods { get; set; }
    }
}
