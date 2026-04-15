using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eikonix.Models
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

        [Column("user_pass")]
        public string hashedpassword { get; set; }

        [Column("user_role")]
        public string userRole { get; set; }

        [Column("user_accountstatus")]
        public string userStatus { get; set; }

        [Column("user_creationdate")]
        public DateTime? userCreationDate { get; set; }
    }
}