using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // Helper to get user ID from username
        private int? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            string userEmail = User.Identity.Name; // From FormsAuthentication
            var user = db.Users.FirstOrDefault(u => u.userEmail == userEmail);

            return user?.userId;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Confirmation(int orderId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Authentication error.";
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                            .Where(o => o.orderId == orderId && o.userId == userId.Value)
                            .Include(o => o.orderItems.Select(oi => oi.product))
                            .FirstOrDefault();

            if (order == null)
            {
                TempData["Error"] = "Order not found or access denied.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        public ActionResult Cart()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                // User not logged in - show all available products or redirect
                var products = db.Products.Where(p => p.productStock > 0).ToList();
                return View(products);
            }

            // Show ONLY products in user's cart
            var cartItems = db.Carts
                .Where(c => c.userId == userId.Value)
                .Include(c => c.Product)
                .ToList();

            var productsInCart = cartItems.Select(c => c.Product).ToList();
            return View(productsInCart);
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

        // GET: Show checkout page (review order)
        [HttpGet]
        public ActionResult Checkout()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please login to checkout";
                return RedirectToAction("Login", "Account");
            }

            // Get cart items with product details
            var cartItems = db.Carts
                .Where(c => c.userId == userId.Value)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Warning"] = "Your cart is empty";
                return RedirectToAction("Cart");
            }

            // Create simple view model
            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems.Select(c => new CartItemViewModel
                {
                    ProductId = c.productId,
                    ProductTitle = c.Product.productTitle,
                    ProductImage = c.Product.productImagePath,
                    ProductArtist = c.Product.productArtist,
                    Price = c.Product.productPrice,
                    Quantity = c.cartQuantity
                }).ToList(),
                TotalAmount = cartItems.Sum(c => c.cartQuantity * c.Product.productPrice)
            };

            return View(viewModel);
        }

        // POST: Process the order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Authentication error. Please log in to complete checkout.";
                return RedirectToAction("Cart");
            }

            var cartItems = db.Carts
                                .Where(c => c.userId == userId.Value)
                                .Include(c => c.Product)
                                .ToList();

            if (!cartItems.Any())
            {
                TempData["Warning"] = "Your cart is empty. Nothing to check out.";
                return RedirectToAction("Cart");
            }

            try
            {
                // Create a new Order
                var newOrder = new Orders
                {
                    userId = userId.Value,
                    orderDate = DateTime.Now,
                    orderStatus = "Processing",
                    orderTotalAmount = 0.0m
                };

                db.Orders.Add(newOrder);
                db.SaveChanges(); // Get orderId

                // Move CartItems to OrderItems and calculate total
                decimal totalAmount = 0.0m;

                foreach (var item in cartItems)
                {
                    var orderItem = new Orderitems
                    {
                        orderId = newOrder.orderId,
                        productId = item.productId,
                        orderItemQuantity = item.cartQuantity,
                        orderItemPrice = item.Product.productPrice
                    };

                    db.Orderitems.Add(orderItem);
                    totalAmount += orderItem.orderItemPrice * orderItem.orderItemQuantity;

                    item.Product.productStock -= item.cartQuantity;
                }

                // Update order total
                newOrder.orderTotalAmount = totalAmount;

                // Remove cart items
                db.Carts.RemoveRange(cartItems);

                // Save all changes
                db.SaveChanges();

                TempData["Success"] = $"Order #{newOrder.orderId} placed!";
                return RedirectToAction("Confirmation", new { orderId = newOrder.orderId });
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = "A server error occurred during order processing.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public JsonResult ReserveProducts(int productId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var product = db.Products.FirstOrDefault(p => p.productId == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Artwork not found." });
            }

            if (product.productStock < 1)
            {
                return Json(new { success = false, message = "Item is out of stock" });
            }

            var existingCartItem = db.Carts.FirstOrDefault(c => c.userId == userId.Value && c.productId == productId);
            if (existingCartItem != null)
            {
                return Json(new { success = false, message = "You have already reserved this artwork." });
            }

            try
            {
                var cartItem = new Cart
                {
                    userId = userId.Value,
                    productId = productId,
                    cartQuantity = 1,
                    ReservationDate = DateTime.Now
                };

                db.Carts.Add(cartItem);
                product.productStock -= 1;
                db.SaveChanges();

                return Json(new { success = true, message = "Artwork successfully reserved and added to your cart." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error during reservation." });
            }
        }

        [HttpPost]
        public JsonResult UnreservedProduct(int productId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Please login first" });
            }

            try
            {
                var cartItem = db.Carts.FirstOrDefault(c => c.userId == userId.Value && c.productId == productId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Artwork not found in your cart." });
                }

                db.Carts.Remove(cartItem);

                var product = db.Products.FirstOrDefault(p => p.productId == productId);
                if (product != null)
                {
                    product.productStock += 1;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Artwork successfully unreserved and restocked." });
                }

                db.SaveChanges();
                return Json(new { success = false, message = "Product not found, but reservation cancelled." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error during unreservation" });
            }
        }
    }
}