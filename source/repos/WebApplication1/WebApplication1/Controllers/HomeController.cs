using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services.Description;
using Org.BouncyCastle.Asn1.Cms;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Checkout()
        {
            string currentUserIdString = User.Identity.Name;


            return View();
        }
        
        public ActionResult Cart()
        {
            var products = db.Products
                    .Where(p => p.productStock > 0)
                    .ToList();
            return View(products);
        }

        
        public ActionResult Digital()
        {
            var products = db.Products
                    .Where(p => p.productStock > 0 && p.productCategory == "Digital Art")
                    .ToList();
            return View(products);
        }

        
        public ActionResult Traditional()
        {
            var products = db.Products
                    .Where(p => p.productStock > 0 && p.productCategory == "Traditional Art")
                    .ToList();
            return View(products);
        }

        
        [HttpPost]

        //i dont know JS and this is not working for some reason :D 
        //idk why but its working now :D

        //for reservation with 1 copy of artworks
        public JsonResult ReserveProducts(int productId)
        {
            string currentUserIdString = User.Identity.Name;

            var product = db.Products.FirstOrDefault(p => p.productId == productId);

            if (product == null)
            {
                return Json(new { sucess = false, message = "Artwork not found." });
            }

            if (product.productStock < 1)
            {
                return Json(new { sucess = false, message = "Item is out of stock(Reserved)" });
            }



            if (int.TryParse(currentUserIdString, out int currentUserIdInt))
            {
                var existingCartItem = db.Carts.FirstOrDefault(c => c.userId == currentUserIdInt && c.productId == productId);
                if (existingCartItem != null)
                {
                    return Json(new { success = false, message = "You have already reserved this artwork." });
                }

                try
                {
                    var cartItem = new WebApplication1.Models.Cart
                    {
                        userId = currentUserIdInt,
                        productId = productId,
                        cartQuantity = 1,
                    };

                    db.Carts.Add(cartItem);
                    product.productStock -= 1;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Artwork successfully reserved and added to your cart." });
                }
                catch (Exception ex)
                {
                    // Log the exception (ex) here for debugging
                    return Json(new { success = false, message = "Server error during reservation." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Error: Invalid user ID format." });
            }
        }


        //on user cancellation
        public JsonResult unreservedProduct(int productId)
        {
            string currentUserIdString = User.Identity.Name;

            if (!int.TryParse(currentUserIdString, out int currentUserIdInt))
            {
                return Json(new { success = false, message = "Error: Invalid user ID format." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // 1. Find the item in the current user's cart
                var cartItem = db.Carts.FirstOrDefault(c => c.userId == currentUserIdInt && c.productId == productId);

                if (cartItem == null)
                {
                    // Maybe it was already removed, or doesn't belong to them
                    return Json(new { success = false, message = "Artwork not found in your cart." }, JsonRequestBehavior.AllowGet);
                }

                // 2. Remove the cart reservation
                db.Carts.Remove(cartItem);

                // 3. Increment stock
                var product = db.Products.FirstOrDefault(p => p.productId == productId);
                if (product != null)
                {
                    product.productStock += 1;
                    db.SaveChanges(); // Commit both cart removal and stock update
                    return Json(new { success = true, message = "Artwork successfully unreserved and restocked." }, JsonRequestBehavior.AllowGet);
                }

                db.SaveChanges(); // If product somehow null, at least remove the cart item

                return Json(new { success = false, message = "Product not found, but reservation cancelled." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return Json(new { success = false, message = "Server error during unreservation" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}