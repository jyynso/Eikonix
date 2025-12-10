using System.Collections.Generic;
using WebApplication1.Data;
using WebApplication1.Models; 

namespace WebApplication1.ViewModels
{
    public class CartViewModel 
    {
        public List<Products> AvailableProducts { get; set; }

        public List<Cart> ReservedCartItems { get; set; }
    }
}