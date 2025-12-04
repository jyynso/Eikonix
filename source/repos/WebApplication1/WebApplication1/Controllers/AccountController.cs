using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            using (var db = new My)
            // Admin authentication
            if (email == "admin@gmail.com" && password == "54321")
            {
                Session["UserEmail"] = email;
                Session["UserRole"] = "Admin";
                return RedirectToAction("Dashboard", "Admin");
            }

            // Regular user authentication
            bool valid = (email == "test@example.com" && password == "12345");

            if (!valid)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            Session["UserEmail"] = email;
            Session["UserRole"] = "User";
            return RedirectToAction("Cart", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string Name, string Email, string Password, string ConfirmPassword, string UserType)
        {
            // Simple validation example
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // TODO: Add logic to save user in DB

            return RedirectToAction("Login");
        }
    }

}