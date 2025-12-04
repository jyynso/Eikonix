using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        // Check if user is admin
        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Dashboard
        public ActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Products Management
        public ActionResult ManageProducts()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Orders Management
        public ActionResult ManageOrders()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Customers Management
        public ActionResult ManageCustomers()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Sales Reports
        public ActionResult SalesReports()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
