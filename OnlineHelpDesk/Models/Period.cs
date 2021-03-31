using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Period
    {
        public Period()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int PeriodId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public string Color { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
