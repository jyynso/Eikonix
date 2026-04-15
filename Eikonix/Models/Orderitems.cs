using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eikonix.Models
    {
        public class Orderitems
        {
            [Key]
            [Column("orderitem_id")]
            public int orderItemId { get; set; }

            // foreign key from orders table
            [Column("order_id")]
            public int orderId { get; set; }

            // foreign key from products table :D
            [Column("product_id")]
            public int productId { get; set; }

            [Column("orderitem_quantity")]
            public int orderItemQuantity { get; set; }

            [Column("orderitem_price")]
            public decimal orderItemPrice { get; set; }

            [ForeignKey("orderId")]
            public virtual Orders Order { get; set; }
            public virtual Products product { get; set; }

    }
    }