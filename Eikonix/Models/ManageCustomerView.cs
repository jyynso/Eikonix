using System;

namespace Eikonix.Models
{
    public class ManageCustomerView
    {
        public int orderId { get; set; }
        public int userId { get; set; }
        public string userEmail {  get; set; } 
        public string userName { get; set; }
        public string userStatus { get; set; }
        public decimal orderTotalAmount { get; set; }
        public string orderStatus { get; set; }
        public DateTime orderDate { get; set; }
        public int totalOrders { get; set; }
        public decimal totalSpent { get; set; }
    }
}