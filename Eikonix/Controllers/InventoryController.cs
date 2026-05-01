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
    }
}