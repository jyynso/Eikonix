using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Cart()
        {
            var products = db.Products
                    .Where(p => p.productStock > 0 || p.productCategory.ToLower() == "digital")
                    .ToList();
            return View(products);
        }
    }
}