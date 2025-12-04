using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using MySql.Data.MySqlClient;
using System.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Data
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