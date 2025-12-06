using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{

    public class AdminController : Controller
    {
        // Check if user is admin
        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "admin";
        }

        // Dashboard
        public ActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            AdminDashboardView model = new AdminDashboardView();

            //display current admin session email
            if (Session["UserEmail"] != null)
            {
                model.CurrentAdminEmail = Session["UserEmail"].ToString();
            }
            else
            {
                model.CurrentAdminEmail = "Admin Account";
            }

            return View(model);
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
