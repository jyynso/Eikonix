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

            //initialize and pull details 
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

            //populate natin :D
            model.ManageProducts = db.Products.ToList();

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
            
            //join two tables 
            model.RecentOrders = (from o in db.Orders
                                  join u in db.Users on o.userId equals u.userId
                                  orderby o.orderDate descending
                                  select new RecentOrderView
                                  {
                                      orderId = o.orderId,
                                      userName = u.userName,
                                      orderDate = o.orderDate,
                                      orderTotalAmount = o.orderTotalAmount,
                                  })
                                 .Take(5)
                                 .ToList();

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

            var customerData = db.Users
                                .Where(u => u.userRole.Equals("customer"))
                                .GroupJoin(
                                    db.Orders,
                                    user => user.userId,
                                    order => order.userId,
                                    (user, userOrders) => new
                                    {
                                        User = user,
                                        allOrders = userOrders
                                    }
                                )
            .Select(c => new ManageCustomerView
            {
                userId = c.User.userId,
                userName = c.User.userName,
                userEmail = c.User.userEmail,
                userStatus = c.User.userStatus,

                totalOrders = c.allOrders.Count(),
                totalSpent = c.allOrders
                                .Where(o => o.orderStatus.Equals("completed"))
                                .Sum(o  => (decimal?)o.orderTotalAmount) ?? 0m
            })
            .ToList();

            model.ManageCustomer = customerData;
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

            model.TotalOrders = db.Orders.Count();
            model.TotalCustomers = db.Users
                                     .Where(u => u.userRole == "customer")
                                     .Count();

            //only completed orders will be total
            var CompletedOrders = db.Orders.Where(o => o.orderStatus.Equals("completed"));
            if (CompletedOrders.Any())
            {
                //total sales
                decimal totalSales = CompletedOrders.Sum(o => o.orderTotalAmount);

                //count only the complete
                int totalCompletedOrders = CompletedOrders.Count();

                model.TotalProducts = totalCompletedOrders;
                model.TotalSales = totalSales;
                model.AvgOrderValue = totalSales / totalCompletedOrders;
            }
            else
            {
                model.AvgOrderValue = 0m;
            }
            if (CompletedOrders.Any())
            {
                model.TotalSales = CompletedOrders.Sum(o => o.orderTotalAmount);
            }
            else
            {
                model.TotalSales = 0;
            }

            //for sales summary by month

            //how many months to show in the table
            const int ShowNumberOfMonths = 5;

            model.MonthSummaryView = new List<AdminSalesReportView>();
            
            for (int i = 0; i < ShowNumberOfMonths; i++)
            {
                DateTime reportDate = DateTime.Today.AddMonths(-i);

                int currentMonth = reportDate.Month;
                int currentYear = reportDate.Year;

                DateTime previousDate = reportDate.AddMonths(-1);
                int previousMonth = previousDate.Month;
                int previousYear = previousDate.Year;

                AdminSalesReportView currentReport = new AdminSalesReportView
                {
                    Month = reportDate.ToString("MMMM yyyy")
                };

                var currentMonthOrders = db.Orders
                        .Where(o => o.orderDate.Month == currentMonth && o.orderDate.Year == currentYear);
                var currentMonthCompletedOrders = currentMonthOrders
                        .Where(o => o.orderStatus.Equals("completed"));
                var previousMonthCompletedOrders = db.Orders
                        .Where(o => o.orderStatus.Equals("completed", StringComparison.OrdinalIgnoreCase) &&
                        o.orderDate.Month == previousMonth &&
                        o.orderDate.Year == previousYear);
                var newCustomersCurrentMonth = db.Users
                    .Where(u => u.userRole.Equals("customer") &&
                        u.userCreationDate.HasValue &&
                        u.userCreationDate.Value.Month == currentMonth &&
                        u.userCreationDate.Value.Year == currentYear);
                decimal previousMonthSales = 0m;

                currentReport.TotalOrders = currentMonthOrders.Count();

                if (currentMonthCompletedOrders.Any())
                {
                    currentReport.TotalSales = currentMonthCompletedOrders.Sum(o => o.orderTotalAmount);
                }
                else
                {
                    currentReport.TotalSales = 0m;
                }

                currentReport.NewCustomers = newCustomersCurrentMonth.Count();

                //we do matematika for growth and shizz
                if (previousMonthCompletedOrders.Any())
                {
                    previousMonthSales = previousMonthCompletedOrders.Sum(o => o.orderTotalAmount);
                }

                if (previousMonthSales > 0)
                {
                    decimal growthRate = (currentReport.TotalSales - previousMonthSales) / previousMonthSales;
                    currentReport.Growth = growthRate;
                }
                else if (currentReport.TotalSales > 0 && previousMonthSales == 0)
                {
                    currentReport.Growth = 1.0m;
                }
                else
                {
                    currentReport.Growth = 0m;
                }

                //uncomment this if gusto lang ishow months with actual data inside them :D
                //if (currentReport.TotalOrders > 0 || currentReport.NewCustomers > 0)
                //{
                //    model.MonthSummaryView.Add(currentReport);
                //}

                //with this, ipapakita natin months with no record, it will fix itself naman new data populating the db
                model.MonthSummaryView.Add(currentReport);

            }
            return View(model);
        }
    }
}