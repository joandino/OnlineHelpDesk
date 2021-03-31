using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Category
    {
        public Category()
        {
            Tickets = new HashSet<Ticket>();
        }

        public int CategoryId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
