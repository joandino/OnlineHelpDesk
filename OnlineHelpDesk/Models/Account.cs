using System;
using System.Collections.Generic;

#nullable disable

namespace OnlineHelpDesk.Models
{
    public partial class Account
    {
        public Account()
        {
            Discussions = new HashSet<Discussion>();
            TicketEmployees = new HashSet<Ticket>();
            TicketSupporters = new HashSet<Ticket>();
        }

        public int AccountId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public bool Status { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Discussion> Discussions { get; set; }
        public virtual ICollection<Ticket> TicketEmployees { get; set; }
        public virtual ICollection<Ticket> TicketSupporters { get; set; }
    }
}
