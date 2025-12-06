using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Data.Entity;


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

            //pull details 
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
                                  orderby o.orderDate descending
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

            return View(model);
        }
    }
}