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

    public class InventoryController : Controller
    {
        // GET: Inventory
        public ActionResult InventoryDashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}