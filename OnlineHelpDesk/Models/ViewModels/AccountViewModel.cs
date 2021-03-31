using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Models.ViewModels
{
    public class AccountViewModel
    {
        public Account Account { get; set; }
        public SelectList Roles { get; set; }
    }
}
