namespace Eikonix.Models
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