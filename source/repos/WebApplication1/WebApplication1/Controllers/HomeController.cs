using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebApplication1.Data;
using WebApplication1.Models;

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

        [HttpPost]
        //this is not working for some reason :D
        //for reservation with 1 copy of artworks
        public JsonResult ReserveProducts(int productId)
        {
            try
            {
                var product = db.Products.FirstOrDefault(p => p.productId == productId);

                if (product == null)
                {
                    return Json(new {success = false, message = "Product not found."}, JsonRequestBehavior.AllowGet);
                }
                
                if (product.productStock > 0)
                {
                    product.productStock -= 1;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Product reserved." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Item is out of stock." }, JsonRequestBehavior.AllowGet);
                }
            }   
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error during reservation" }, JsonRequestBehavior.AllowGet);
            }
        }
        //on user cancellation
        public JsonResult unreservedProduct(int productId)
        {
            try
            {
                var product = db.Products.FirstOrDefault(p => p.productId == productId);

                if (product != null)
                {
                    product.productStock += 1;
                    db.SaveChanges();

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Product not found for unreservation." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error during unreservation" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}