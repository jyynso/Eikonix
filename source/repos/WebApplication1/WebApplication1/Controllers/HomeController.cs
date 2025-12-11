using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        private int? GetCurrentUserId()
        {
            //pls workk
            return Session["UserId"] as int?;

            //wag muna ito ibalik

            //if (!User.Identity.IsAuthenticated)
            //    return null;

            //string userEmail = User.Identity.Name; // From FormsAuthentication
            //var user = db.Users.FirstOrDefault(u => u.userEmail == userEmail);

            //return user?.userId;
        }

        //debug for now to see if we are actually getting the userid from session kasi ayaw gumana eiiii
        public ActionResult DebugAuth()
        {
            var info = new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserIdentityName = User.Identity.Name ?? "NULL",
                AuthenticationType = User.Identity.AuthenticationType,
                SessionUserId = Session["UserId"],
                SessionUserEmail = Session["UserEmail"],
                SessionUserRole = Session["UserRole"]
            };

            return Json(info, JsonRequestBehavior.AllowGet);
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
            List<Cart> reservedItems = new List<Cart>();
            List<Products> availableProducts;
            List<Products> allInStockProducts = db.Products
                .Where(p => p.productStock > 0)
                .ToList();

            //FIX1
            //List<Products> availableProducts = allInStockProducts;

            availableProducts = allInStockProducts;

            if (userId.HasValue)
            {
                reservedItems = db.Carts
                    .Where(c => c.userId == userId.Value)
                    .Include(c => c.Product)
                    .ToList();

                var reservedProductIds = reservedItems.Select(c => c.productId).ToList();


                //FIX2
                availableProducts = allInStockProducts
                    .Where(p => !reservedProductIds.Contains(p.productId))
                    .ToList();
            }

            var viewModel = new CartViewModel
            {
                AvailableProducts = availableProducts,
                ReservedCartItems = reservedItems
            };

            return View(viewModel);
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

        [HttpGet]
        public ActionResult Checkout()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Please login to checkout";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = db.Carts
                .Where(c => c.userId == userId.Value)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Warning"] = "Your cart is empty";
                return RedirectToAction("Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems.Select(c => new CartItemViewModel
                {
                    ProductId = c.productId,
                    ProductTitle = c.Product.productTitle,
                    ProductImage = c.Product.productImagePath,
                    ProductSize = c.Product.productSize,
                    ProductArtist = c.Product.productArtist,
                    Price = c.Product.productPrice,
                    ProductStock = c.cartQuantity
                }).ToList(),
                TotalAmount = cartItems.Sum(c => c.cartQuantity * c.Product.productPrice)
            };

            return View(viewModel);
        }

        public ActionResult Search(string q) 
        {
            List<WebApplication1.Data.Products> products;

            if (!string.IsNullOrWhiteSpace(q))
            {
                string searchTerm = q.Trim().ToLower();

                products = db.Products
                    .Where(p => p.productStock > 0 &&
                                (p.productTitle.ToLower().Contains(searchTerm) ||
                                 p.productArtist.ToLower().Contains(searchTerm) ||
                                 p.productDescription.ToLower().Contains(searchTerm)))
                    .ToList(); 
            }
            else
            {
                products = new List<WebApplication1.Data.Products>();
            }

            ViewBag.SearchQuery = q;

            return View("SearchResults", products);
        }

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
                var newOrder = new Orders
                {
                    userId = userId.Value,
                    orderDate = DateTime.Now,
                    orderStatus = "Processing",
                    orderTotalAmount = 0.0m
                };

                db.Orders.Add(newOrder);
                db.SaveChanges();

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

                newOrder.orderTotalAmount = totalAmount;

                db.Carts.RemoveRange(cartItems);

                db.SaveChanges();

                TempData["Success"] = $"Order #{newOrder.orderId} placed!";
                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A server error occurred during order processing.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        [ActionName("ReserveProducts")]
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
                };

                db.Carts.Add(cartItem);
                db.SaveChanges();

                return Json(new { success = true, message = "Artwork successfully reserved and added to your cart." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error during reservation." });
            }
        }


        [HttpPost]
        [ActionName("UnreserveProduct")]
        public JsonResult UnreserveProduct(int productId)
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
                    //product.productStock += 1;
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