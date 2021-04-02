using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Models.ViewModels
{
    public class SubCategoryViewModel
    {
        public SubCategory SubCategory { get; set; }
        public SelectList Categories { get; set; }
    }
}
