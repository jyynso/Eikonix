using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Orders
    {
        [Key]
        [Column("order_id")]
        public int orderId { get; set; }

        //foreign key from users table
        [Column("user_id")]
        public int userId { get; set; }

        [Column("order_totalamount")]
        public decimal orderTotalAmount { get; set; }

        [Column("order_status")]
        public string orderStatus { get; set; }

        [Column("order_creationdate")]
        public DateTime orderDate { get; set; }

        public virtual ICollection<Orderitems> orderItems { get; set; }
    }
}