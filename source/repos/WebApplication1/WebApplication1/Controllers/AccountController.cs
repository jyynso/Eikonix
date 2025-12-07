using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Helpers;

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
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.userEmail == email);
                if (user.userStatus.Equals("inactive", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = "Your account is currently inactive. Please contact the administrator.";
                    return View();
                }

                if (user != null && Utils.VerifyPassword(password, user.hashedpassword))
                {
                    Session["UserId"] = user.userId; //idk why I put this here but it may come useful later :D
                    Session["UserEmail"] = user.userEmail;
                    Session["UserRole"] = user.userRole;

                    if (user.userRole == "admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else
                        return RedirectToAction("Cart", "Home");
                }
            }
            ViewBag.Error = "Invalid Email or Password";
            return View();
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
        public ActionResult Register(string name, string email, string password, string ConfirmPassword, string userRole)
        {
            // Simple validation example
            if (password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            using (var db = new AppDbContext())
            {
                string hashed = Utils.HashPassword(password);

                var user = new Users
                {
                    userName = name,
                    userEmail = email,
                    hashedpassword = hashed,
                    userRole = string.IsNullOrEmpty(userRole) ? "customer" : userRole
                };

                db.Users.Add(user);
                db.SaveChanges();
            }

            // TODO: Add logic to save user in DB
            // done.

            return RedirectToAction("Login");
        }
    }

}