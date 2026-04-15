using System.Collections.Generic;

namespace Eikonix.Models
{
    public class CartViewModel 
    {
        public List<Products> AvailableProducts { get; set; }

        public List<Cart> ReservedCartItems { get; set; }
    }
}