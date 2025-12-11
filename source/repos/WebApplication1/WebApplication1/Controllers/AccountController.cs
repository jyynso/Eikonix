using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Helpers;
using System.Web.Security;
using System.Web.UI.WebControls;

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
        public ActionResult Login(string email, string password, bool RememberMe)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.userEmail == email);

                if (user != null && Utils.VerifyPassword(password, user.hashedpassword))
                {
                    FormsAuthentication.SetAuthCookie(user.userEmail, false);

                    Session["UserId"] = user.userId; //idk why I put this here but it may come useful later :D 
                                                    //update: it is useful :D
                    Session["UserEmail"] = user.userEmail;
                    Session["UserRole"] = user.userRole;

                    //"Maalaala Mo Kaya?"
                    var expiration = RememberMe ?
                        DateTime.Now.AddDays(7) :
                        DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes);

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            1,
                            user.userEmail,
                            DateTime.Now,
                            expiration,
                            RememberMe,
                            user.userRole   ,
                            FormsAuthentication.FormsCookiePath

                        );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                    if (RememberMe)
                    {
                        authCookie.Expires = ticket.Expiration;
                    }

                    Response.Cookies.Add(authCookie);


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

            FormsAuthentication.SignOut();
            
            Session.Clear();
            Session.Abandon();
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