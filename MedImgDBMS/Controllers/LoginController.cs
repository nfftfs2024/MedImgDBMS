using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MedImgDBMS.Models;

namespace MedImgDBMS.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        //static List<account> acct = new List<account>();
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(account objUser)
        {
            string message = string.Empty;      // Create empty string

            if (ModelState.IsValid)
            {
                using (pjmedimgdbEntities db = new pjmedimgdbEntities())
                {
                    var obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName) && a.AcctPasswd.Equals(objUser.AcctPasswd)).FirstOrDefault();    // Compare account name and passwd with DB
                    if (obj != null)
                    {
                        Session["UserID"] = obj.AcctID.ToString();          // Get session user id
                        Session["AcctName"] = obj.AcctLName.ToString();     // Get session account name

                        string role = obj.user.UserRoleID.ToString();       
                        Session["UserRole"] = role;                         // Get session user role

                        if (role == "1")                                    // Redirect users to different pages
                        {
                            return RedirectToAction("AdminDashBoard");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Images");
                        }

                        // Backup for different list pages for doctors and experts
                        //else if (role == "3")
                        //{
                        //    return RedirectToAction("Index", "Images");
                        //}
                    }
                }
            }
            return View(objUser);
        }

        public ActionResult AdminDashBoard()             // For backup admin page
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout()                    // For system logout
        {
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}