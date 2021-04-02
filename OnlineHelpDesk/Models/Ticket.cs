using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Ticket
    {
        public Ticket()
        {
            Discussions = new HashSet<Discussion>();
            Photos = new HashSet<Photo>();
        }

        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int PeriodId { get; set; }
        public int? EmployeeId { get; set; }
        public int? SupporterId { get; set; }

        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public virtual Account Employee { get; set; }
        public virtual Period Period { get; set; }
        public virtual Status Status { get; set; }
        public virtual Account Supporter { get; set; }
        public virtual ICollection<Discussion> Discussions { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }
}
