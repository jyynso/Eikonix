using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Data;


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

        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [ForeignKey("userId")]
        public virtual Users User { get; set; }

        [ForeignKey("productId")]
        public virtual Products Product { get; set; }
    }
}