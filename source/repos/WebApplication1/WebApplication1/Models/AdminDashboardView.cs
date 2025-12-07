using System.Collections.Generic;
using WebApplication1.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class AdminDashboardView
    {
        //to display current session email
        public string CurrentAdminEmail { get; set; }
        
        //admin dashboard details :D
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }

        //table details

        //dashboard and manage orders view
        public List<RecentOrderView> RecentOrders { get; set; }

        //manage products
        public List<Products> ManageProducts { get; set; }

        //manage customers
        public List<ManageCustomerView> ManageCustomer { get; set; }

        //sales report
        public decimal AvgOrderValue { get; set; }
        public List<AdminSalesReportView> MonthSummaryView { get; set; }
        public List<AdminTopProductsView> TopProducts { get; set; }
    }
}