using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eikonix.Models
{
    [Table("cart")] // <-- FIX: Explicitly map the C# model to the exact MySQL table name (singular, lowercase)
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

        [ForeignKey("userId")]
        public virtual Users User { get; set; }

        [ForeignKey("productId")]
        public virtual Products Product { get; set; }
    }
}