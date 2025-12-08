using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Data.Entity;
using System.Security.Cryptography;
using System.IO;


namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        protected AppDbContext db = new AppDbContext();

        // check if user is admin
        protected bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString().Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserEmail"] != null)
            {
                ViewBag.CurrentAdminEmail = Session["UserEmail"].ToString();
            }
            else
            {
                ViewBag.CurrentAdminEmail = "Admin Account";
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class AdminController : BaseController
    {
        // Dashboard
        public ActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            AdminDashboardView model = new AdminDashboardView();

            //initialize and pull details 
            model.TotalProducts = db.Products.Count();
            model.TotalOrders = db.Orders.Count();

            //only completed orders will be total
            var CompletedOrders = db.Orders.Where(o => o.orderStatus.Equals("completed"));

            if (CompletedOrders.Any())
            {
                model.TotalSales = CompletedOrders.Sum(o => o.orderTotalAmount);
            }
            else
            {
                model.TotalSales = 0;
            }
            model.TotalCustomers = db.Users
                                     .Where(u => u.userRole == "customer")
                                     .Count();
            model.RecentOrders = (from o in db.Orders
                                  join u in db.Users on o.userId equals u.userId
                                  orderby o.orderDate ascending
                                  select new RecentOrderView
                                  {
                                      orderId = o.orderId,
                                      userName = u.userName,
                                      orderStatus = o.orderStatus,
                                      orderDate = o.orderDate,
                                      orderTotalAmount = o.orderTotalAmount,
                                  })
                                  .Take(5)
                                  .ToList();
            return View(model);
        }

        // Products Management
        public ActionResult ManageProducts()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            AdminDashboardView model = new AdminDashboardView();

            //populate natin :D
            model.ManageProducts = db.Products.ToList();

            return View(model);
        }

        // Orders Management
        public ActionResult ManageOrders()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            AdminDashboardView model = new AdminDashboardView();

            //join two tables 
            model.RecentOrders = (from o in db.Orders
                                  join u in db.Users on o.userId equals u.userId
                                  orderby o.orderDate ascending
                                  select new RecentOrderView
                                  {
                                      orderId = o.orderId,
                                      userName = u.userName,
                                      orderStatus = o.orderStatus,
                                      orderDate = o.orderDate,
                                      orderTotalAmount = o.orderTotalAmount,
                                  })
                                 .Take(5)
                                 .ToList();

            return View(model);
        }

        // Customers Management
        public ActionResult ManageCustomers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            AdminDashboardView model = new AdminDashboardView();

            var customerData = db.Users
                                .Where(u => u.userRole.Equals("customer"))
                                .GroupJoin(
                                    db.Orders,
                                    user => user.userId,
                                    order => order.userId,
                                    (user, userOrders) => new
                                    {
                                        User = user,
                                        allOrders = userOrders
                                    }
                                )
            .Select(c => new ManageCustomerView
            {
                userId = c.User.userId,
                userName = c.User.userName,
                userEmail = c.User.userEmail,
                userStatus = c.User.userStatus,

                totalOrders = c.allOrders.Count(),
                totalSpent = c.allOrders
                                .Where(o => o.orderStatus.Equals("completed"))
                                .Sum(o => (decimal?)o.orderTotalAmount) ?? 0m
            })
            .ToList();

            model.ManageCustomer = customerData;
            return View(model);
        }

        // Sales Reports
        public ActionResult SalesReports()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            AdminDashboardView model = new AdminDashboardView();

            model.TotalOrders = db.Orders.Count();
            model.TotalCustomers = db.Users
                                     .Where(u => u.userRole == "customer")
                                     .Count();
            //only completed orders will be total
            var CompletedOrders = db.Orders.Where(o => o.orderStatus.Equals("completed"));

            if (CompletedOrders.Any())
            {
                //total sales
                decimal totalSales = CompletedOrders.Sum(o => o.orderTotalAmount);

                //count only the complete
                int totalCompletedOrders = CompletedOrders.Count();

                model.TotalProducts = totalCompletedOrders;
                model.TotalSales = totalSales;
                model.AvgOrderValue = totalSales / totalCompletedOrders;
            }
            else
            {
                model.AvgOrderValue = 0m;
            }
            if (CompletedOrders.Any())
            {
                model.TotalSales = CompletedOrders.Sum(o => o.orderTotalAmount);
            }
            else
            {
                model.TotalSales = 0;
            }

            //for sales summary by month

            //how many months to show in the table
            const int ShowNumberOfMonths = 5;

            model.MonthSummaryView = new List<AdminSalesReportView>();

            for (int i = 0; i < ShowNumberOfMonths; i++)
            {
                DateTime reportDate = DateTime.Today.AddMonths(-i);

                int currentMonth = reportDate.Month;
                int currentYear = reportDate.Year;

                DateTime previousDate = reportDate.AddMonths(-1);
                int previousMonth = previousDate.Month;
                int previousYear = previousDate.Year;

                AdminSalesReportView currentReport = new AdminSalesReportView
                {
                    Month = reportDate.ToString("MMMM yyyy")
                };

                var currentMonthOrders = db.Orders
                        .Where(o => o.orderDate.Month == currentMonth && o.orderDate.Year == currentYear);
                var currentMonthCompletedOrders = currentMonthOrders
                        .Where(o => o.orderStatus.Equals("completed"));
                var previousMonthCompletedOrders = db.Orders
                        .Where(o => o.orderStatus.Equals("completed", StringComparison.OrdinalIgnoreCase) &&
                        o.orderDate.Month == previousMonth &&
                        o.orderDate.Year == previousYear);
                var newCustomersCurrentMonth = db.Users
                    .Where(u => u.userRole.Equals("customer") &&
                        u.userCreationDate.HasValue &&
                        u.userCreationDate.Value.Month == currentMonth &&
                        u.userCreationDate.Value.Year == currentYear);
                decimal previousMonthSales = 0m;

                currentReport.TotalOrders = currentMonthOrders.Count();

                if (currentMonthCompletedOrders.Any())
                {
                    currentReport.TotalSales = currentMonthCompletedOrders.Sum(o => o.orderTotalAmount);
                }
                else
                {
                    currentReport.TotalSales = 0m;
                }

                currentReport.NewCustomers = newCustomersCurrentMonth.Count();

                //we do matematika for growth and shizz
                if (previousMonthCompletedOrders.Any())
                {
                    previousMonthSales = previousMonthCompletedOrders.Sum(o => o.orderTotalAmount);
                }

                if (previousMonthSales > 0)
                {
                    decimal growthRate = (currentReport.TotalSales - previousMonthSales) / previousMonthSales;
                    currentReport.Growth = growthRate;
                }
                else if (currentReport.TotalSales > 0 && previousMonthSales == 0)
                {
                    currentReport.Growth = 1.0m;
                }
                else
                {
                    currentReport.Growth = 0m;
                }

                //uncomment this and comment the one below this if gusto lang ishow months with actual data inside them :D
                //if (currentReport.TotalOrders > 0 || currentReport.NewCustomers > 0)
                //{
                //    model.MonthSummaryView.Add(currentReport);
                //}

                //with this, ipapakita natin months with no record, it will fix itself naman new data populating the db
                model.MonthSummaryView.Add(currentReport);
            }

            //top selling sining gawa

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
            //this is a problem
            //we need to add this to not make the market share& but by doing so the total sales and other data is setting to 0
            //model.TotalProducts = 0;
            //model.TotalSales = 0;
            //model.AvgOrderValue = 0;

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

            //category performance
            decimal overallTotalRevenue = db.Orderitems
                                            .Where(oi => oi.Order.orderStatus.Equals("completed"))
                                            .Sum(oi => (decimal?)oi.orderItemQuantity * oi.orderItemPrice) ?? 0m;
            var CompletedCategorySalesData = db.Products
                                                .Join(db.Orderitems,
                                                    p => p.productId,
                                                    oi => oi.productId,
                                                    (p, oi) => new { Product = p, OrderItem = oi }
                                                    )
                                                .Join(db.Orders,
                                                    combined => combined.OrderItem.orderId,
                                                    o => o.orderId,
                                                    (combined, o) => new
                                                    {
                                                        Product = combined.Product,
                                                        OrderItem = combined.OrderItem,
                                                        Order = o
                                                    }
                                                 )
                                                .Where(x => x.Order.orderStatus.Equals("completed"))
                                                .ToList();
            var categoryPerformance = CompletedCategorySalesData
                                        .GroupBy(x => x.Product.productCategory)
                                        .Select(g => new
                                        {
                                            CategoryName = g.Key,
                                            TotalProducts = g.Select(x => x.Product.productId).Distinct().Count(),
                                            TotalUnitsSold = g.Sum(x => x.OrderItem.orderItemQuantity),
                                            TotalRevenue = g.Sum(x => x.OrderItem.orderItemQuantity * x.OrderItem.orderItemPrice),
                                        })
                                        .ToList();
            model.CategoryPerformance = new List<AdminCategoryPerformanceView>();

            foreach (var cat in categoryPerformance)
            {
                AdminCategoryPerformanceView category = new AdminCategoryPerformanceView
                {
                    CategoryName = cat.CategoryName,
                    TotalProducts = db.Products.Count(p => p.productCategory == cat.CategoryName),
                    TotalUnitsSold = cat.TotalUnitsSold,
                    TotalRevenue = cat.TotalRevenue
                };

                category.AveragePrice = category.TotalUnitsSold > 0
                          ? category.TotalRevenue / category.TotalUnitsSold
                          : 0m;

                if (overallTotalRevenue > 0)
                {
                    category.MarketSharePercentage = (category.TotalRevenue / overallTotalRevenue) * 100m;
                }
                else
                {
                    category.MarketSharePercentage = 0m;
                }
                model.CategoryPerformance.Add(category);
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult GetOrderDetails(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.orderId == id);

            if (order == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Order not found." }, JsonRequestBehavior.AllowGet);
            }

            var customer = db.Users.FirstOrDefault(u => u.userId == order.userId);
            string customerName = customer?.userName ?? "N/A";
            string customerEmail = customer?.userEmail ?? "N/A";

            var items = db.Orderitems
                .Where(oi => oi.orderId == id)
                .Join(db.Products, 
                      oi => oi.productId,
                      p => p.productId,
                      (oi, p) => new
                      {
                          oi.orderItemQuantity,
                          oi.orderItemPrice,
                          productTitle = p.productTitle
                      })
                .ToList();

            var responseData = new
            {
                order.orderId,
                order.orderTotalAmount,
                order.orderStatus,
                order.orderDate,
                userName = customerName,
                userEmail = customerEmail,
                Items = items                        
            };

            return Json(new { success = true, data = responseData }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        //for user status active or inactive
        public ActionResult ToggleUserStatus(int id)
        {
            var userToggle = db.Users.Find(id);
            if (userToggle == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "User not found." });
            }

            string currentStatus = userToggle.userStatus.ToLower();
            string newStatus;
            string message;

            if (currentStatus == "active")
            {
                newStatus = "inactive";
                message = $"Customer #{id} has been deactivated.";
            }
            else
            {
                newStatus = "active";
                message = $"Customer #{id} has been activated.";
            }
            userToggle.userStatus = newStatus;
            db.Entry(userToggle).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, newStatus = newStatus, message = message });
        }


        //for the order status
        public ActionResult UpdateOrderStatus(int id, string newStatus)
        {

            var order = db.Orders.Find(id);

            if (order == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Order not found." });
            }


            string statusLower = newStatus.ToLower();
            var validStatuses = new List<string> { "pending", "processing", "completed"};

            if (!validStatuses.Contains(statusLower))
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = $"Invalid status provided: {newStatus}" });
            }

            order.orderStatus = newStatus;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, message = $"Order #{id} status updated to: {newStatus}" });
        }

        //for add product button
        public ActionResult AddProduct(Products product, HttpPostedFileBase productImage)
        {

            if (ModelState.IsValid)
            {
                if (productImage != null && productImage.ContentLength > 0)
                {
                    try
                    {
                        string path = Server.MapPath("~/Content/imgs/Drawing/");

                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }

                        string fileExtension = Path.GetExtension(productImage.FileName);
                        string fileName = Guid.NewGuid().ToString() + fileExtension;
                        string fullPath = Path.Combine(path, fileName);

                        productImage.SaveAs(fullPath);

                        product.productImagePath = "~/Content/imgs/Drawing/" + fileName;

                        product.productCreationDate = DateTime.Now;

                        db.Products.Add(product);
                        db.SaveChanges();

                        return Json(new { success = true, message = "Product added successfully!" });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Error saving product or file: " + ex.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Image file is required." });
                }
            }

            return Json(new { success = false, message = "Invalid product data provided." });
        }

        //view product details
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

        //edit the product
        public JsonResult EditProduct(Products product, HttpPostedFileBase productImage)
        {
            try
            {
                // 1. Get the original product entity
                var existingProduct = db.Products.Find(product.productId);

                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Product not found for update." });
                }

                // 2. Update properties manually (safer than using Attach/Entry.State)
                existingProduct.productTitle = product.productTitle;
                existingProduct.productDescription = product.productDescription;
                existingProduct.productPrice = product.productPrice;
                existingProduct.productStock = product.productStock;
                // ... update all other fields ...

                // 3. Handle image update (if a new file was uploaded)
                if (productImage != null && productImage.ContentLength > 0)
                {
                    // TO DO: Add image saving logic here, update existingProduct.productImagePath
                }

                db.SaveChanges();

                return Json(new { success = true, message = $"{product.productTitle} updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving product or file: " + ex.Message });
            }
        }

        //burahin
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