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

            return View();
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