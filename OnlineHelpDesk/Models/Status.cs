using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Status
    {
        public Status()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int StatusId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool Display { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
