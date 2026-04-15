using System.Data.Entity;
using Eikonix.Models;

namespace Eikonix.Data
{
    public class AppDbContext : DbContext
    {   
        public AppDbContext() : base("MySqlConnection") { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Orderitems> Orderitems { get; set; }
    }
}