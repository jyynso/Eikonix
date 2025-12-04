using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Data
{
    public class Products
    {
        public int product_id { get; set; }
        public string product_title { get; set; }
        public string product_description { get; set; }
        public string product_size { get; set; }
        public enum product_category { digital, traditional }
        public string product_medium { get; set; }
        public string product_softwareused { get; set; }
        public string product_artist { get; set; }
        public decimal product_price { get; set; }
        public int product_stock { get; set; }
        public string product_imagepath { get; set; }

    }
}