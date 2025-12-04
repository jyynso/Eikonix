using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApplication1.Models
{
    public class Users
    {
        [Key]
        [Column("user_id")]
        public int userId { get; set; }

        [Column("user_name")]
        public string userName{ get; set; }

        [Column("user_email")]
        public string userEmail { get; set; }

        [Column("user_role")]
        public string userRole { get; set; }

        [Column("user_accountstatus")]
        public string userAccountStatus {get; set; }

    }
}