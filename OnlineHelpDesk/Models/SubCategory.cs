using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineHelpDesk.Models
{
    public class SubCategory
    {
        public int SubCategoryId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public bool Status { get; set; }
        public virtual Category Category { get; set; }
    }
}
