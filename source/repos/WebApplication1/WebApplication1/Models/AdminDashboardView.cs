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

        //table details
        public List<Orders> RecentOrders { get; set; }
    }
}