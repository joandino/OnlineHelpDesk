using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Discussion
    {
        public int DiscussionId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int TicketId { get; set; }
        public int AccountId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
