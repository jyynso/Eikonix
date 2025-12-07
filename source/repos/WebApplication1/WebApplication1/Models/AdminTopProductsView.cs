    using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class AdminTopProductsView
    {
        public int Rank { get; set; }
        public string ArtworkName { get; set; }
        public string Category { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }

        public string ExcellentRevenue { get; set; }    
        public string GoodRevenue {get; set; }
        public string PoorRevenue { get; set; }

        public string PerformanceLabel { get; set; }
        public string PerformanceClass { get; set; }

    }
}