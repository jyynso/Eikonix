// CheckoutViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Asn1.Crmf;

namespace WebApplication1.Models
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImage { get; set; }
        public string ProductSize { get; set; }
        public string ProductArtist { get; set; }
        public decimal Price { get; set; }
        public int ProductStock { get; set; }
        public decimal ItemTotal => Price * ProductStock;
    }
}