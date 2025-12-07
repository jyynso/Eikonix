using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Org.BouncyCastle.Asn1.Mozilla;

namespace WebApplication1.Models
{
    public class AdminSalesReportView
    {
        public string Month { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; } 
        public int NewCustomers { get; set; }   
        public decimal Growth {  get; set; }    

    }
}