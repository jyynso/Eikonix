using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eikonix.Models
{
    public class Products
    {
        [Key]
        [Column("product_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int productId { get; set; }

        [Column("product_title")]
        public string productTitle { get; set; }

        [Column("product_description")]
        public string productDescription { get; set; }

        [Column("product_size")]
        public string productSize { get; set; }

        [Column("product_category")]
        public string productCategory { get; set; }

        [Column("product_medium")]
        public string productMedium { get; set; }

        [Column("product_softwareused")]
        public string productSoftwareUsed { get; set; }

        [Column("product_artist")]
        public string productArtist { get; set; }

        [Column("product_price")]
        public decimal productPrice { get; set; }

        [Column("product_stock")]
        public int productStock { get; set; }

        [Column("product_imagepath")]
        public string productImagePath { get; set; }

        [Column("product_creationdate")]
        public DateTime productCreationDate { get; set; }

    }
}