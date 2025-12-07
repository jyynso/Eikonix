using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class AdminCategoryPerformanceView
    {
        public string CategoryName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MarketSharePercentage { get; set; }
        public string MarketShareClass { get; set; }
    }
}