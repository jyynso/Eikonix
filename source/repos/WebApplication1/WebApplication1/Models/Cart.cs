using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApplication1.Models
{
    public class Cart
    {
        [Key]
        [Column("cart_id")]
        public int cartId { get; set; }

        //foreign key from users table
        [Column("user_id")] 
        public int userId { get; set; }

        //foreign key from products table
        [Column("product_id")]
        public int productId { get; set; }

        [Column("cart_quantity")]
        public int cartQuantity { get; set; }
    }
}