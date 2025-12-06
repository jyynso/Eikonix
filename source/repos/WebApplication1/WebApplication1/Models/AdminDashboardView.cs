using System.Collections.Generic;
using WebApplication1.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class AdminDashboardView
    {
        public string CurrentAdminEmail { get; set; }
        public List<Products> Products { get; set; }
        public List<Users> Users { get; set; }
    }
}