using MySql.Data.MySqlClient;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace WebApplication1.Data
{
    public class MySQLContext
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public IDbConnection connection => new MySqlConnection(connectionString);
    }

}