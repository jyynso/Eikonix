using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eikonix.Data;
using Eikonix.Models;

namespace Eikonix.Controllers
{
    public class InventoryBaseController : Controller
    {
        protected AppDbContext db = new AppDbContext();

        // check if user is supplier
        protected bool IsSupplier()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString().Equals("supplier", StringComparison.OrdinalIgnoreCase);
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserEmail"] != null)
            {
                ViewBag.CurrentAdminEmail = Session["UserEmail"].ToString();
            }
            else
            {
                ViewBag.CurrentAdminEmail = "Supplier Account";
            }

            base.OnActionExecuting(filterContext);
        }
    }
    public class InventoryController : InventoryBaseController
    {
        // GET: Inventory
        public ActionResult InventoryDashboard()
        {
            // supplier ba ikaw? 
            if (!IsSupplier())
            {
                return RedirectToAction("Login", "Account");
            }

            AdminDashboardView model = new AdminDashboardView();

            //initialize and get details
            model.TotalProducts = db.Products.Count();
            //we only count completed orders for the total sales reflection
            var CompletedOrders = db.Orders.Where(o => o.orderStatus.Equals("completed"));

            if (CompletedOrders.Any())
            {
          
                model.TotalSales = CompletedOrders.Sum(o => o.orderTotalAmount);
            }
            else
            {
                model.TotalSales = 0;
            }

            var topProducts = db.Orderitems
                                .Where(oi => oi.Order.orderStatus.Equals("completed"))
                                .GroupBy(oi => oi.productId)
                                .Select(g => new
                                {
                                    productId = g.Key,
                                    unitsSold = g.Sum(oi => oi.orderItemQuantity),
                                    revenue = g.Sum(oi => oi.orderItemQuantity * oi.orderItemPrice)
                                })
                                .OrderByDescending(x => x.revenue)
                                .Take(5)
                                .ToList();

            //threshold
            const decimal ExcellentRevenue = 200000.00m;
            const decimal GoodRevenue = 100000.00m;

            int RankCounter = 1;
            var rankings = topProducts
                    .Join(db.Products,
                        rank => rank.productId,
                        product => product.productId,
                        (rank, product) => new AdminTopProductsView
                        {
                            ArtworkName = product.productTitle,
                            Category = product.productCategory,
                            UnitsSold = rank.unitsSold,
                            Revenue = rank.revenue,
                            PerformanceLabel = "",
                            PerformanceClass = ""
                        })
                    .ToList();

            foreach (var item in rankings)
            {
                item.Rank = RankCounter++;
                if (item.Revenue >= ExcellentRevenue)
                {
                    item.PerformanceLabel = "EXCELLENT";
                    item.PerformanceClass = "status-completed";
                }
                else if (item.Revenue >= GoodRevenue)
                {
                    item.PerformanceLabel = "GOOD";
                    item.PerformanceClass = "status-processing";
                }
                else
                {
                    item.PerformanceLabel = "FAIR";
                    item.PerformanceClass = "status-pending";
                }
            }
            model.TopProducts = rankings;

            model.ManageProducts = db.Products.ToList();

            return View(model);
        }

        public ActionResult InventoryProducts()
        {
            if (!IsSupplier())
            {
                return RedirectToAction("Login", "Account");
            }
            AdminDashboardView model = new AdminDashboardView();

            model.ManageProducts = db.Products.ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(string productTitle, string productDescription, decimal productPrice, int productStock, string productCategory, string productMedium, string productSize, string productArtist, string productSoftwareUsed, HttpPostedFileBase productImage)
        {
            try
            {
                if (string.IsNullOrEmpty(productTitle))
                {
                    return Json(new { success = false, message = "Title is required." });
                }

                // Robust check for duplicate title (case-insensitive and trimmed)
                var isDuplicate = db.Products.Any(p => p.productTitle.Trim().ToLower() == productTitle.Trim().ToLower());

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "An artwork with this title already exists. Please use a unique name." });
                }

                var product = new Products
                {
                    productTitle = productTitle,
                    productDescription = productDescription,
                    productPrice = productPrice,
                    productStock = productStock,
                    productCategory = productCategory,
                    productMedium = productMedium,
                    productSize = productSize,
                    productArtist = productArtist,
                    productSoftwareUsed = productSoftwareUsed,
                    productCreationDate = DateTime.Now,
                    productImagePath = "~/Content/imgs/Drawing/p1.png" // Placeholder
                };

                if (productImage != null && productImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/imgs/Drawing/"), fileName);
                    productImage.SaveAs(path);
                    product.productImagePath = "~/Content/imgs/Drawing/" + fileName;
                }

                db.Products.Add(product);
                db.SaveChanges();

                return Json(new { success = true, message = "Product added successfully!" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " Details: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += " " + ex.InnerException.InnerException.Message;
                    }
                }
                return Json(new { success = false, message = "Error: " + errorMessage });
            }
        }

        public JsonResult GetProductDetails(int id)
        {
            try
            {
                var product = db.Products.Find(id);

                if (product == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Product not found." }, JsonRequestBehavior.AllowGet);
                }
                var productData = new
                {
                    productId = product.productId,
                    productTitle = product.productTitle,
                    productDescription = product.productDescription,
                    productSize = product.productSize,
                    productCategory = product.productCategory,
                    productMedium = product.productMedium,
                    productSoftwareUsed = product.productSoftwareUsed,
                    productArtist = product.productArtist,
                    productPrice = product.productPrice,
                    productStock = product.productStock,
                    productImagePath = product.productImagePath,
                    productCreationDate = product.productCreationDate.ToString("yyyy-MM-dd HH:mm:ss")
                };
                return Json(new { success = true, product = productData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EditProduct(Products product, HttpPostedFileBase productImage)
        {
            try
            {
                var existingProduct = db.Products.Find(product.productId);

                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found for update." });
                }

                var isDuplicate = db.Products.Any(p => p.productId != product.productId && p.productTitle.Trim().ToLower() == product.productTitle.Trim().ToLower());

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "Another artwork already has this title. Please use a unique name." });
                }

                existingProduct.productTitle = product.productTitle;
                existingProduct.productDescription = product.productDescription;
                existingProduct.productPrice = product.productPrice;
                existingProduct.productStock = product.productStock;
                existingProduct.productCategory = product.productCategory;
                existingProduct.productMedium = product.productMedium;
                existingProduct.productSize = product.productSize;
                existingProduct.productArtist = product.productArtist;
                existingProduct.productSoftwareUsed = product.productSoftwareUsed;

                if (productImage != null && productImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/imgs/Drawing/"), fileName);
                    productImage.SaveAs(path);
                    existingProduct.productImagePath = "~/Content/imgs/Drawing/" + fileName;
                }

                db.SaveChanges();

                return Json(new { success = true, message = $"{product.productTitle} updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving product: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }

                db.Products.Remove(product);
                db.SaveChanges();

                return Json(new { success = true, message = $"{product.productTitle} deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting product: " + ex.Message });
            }
        }
    }
}