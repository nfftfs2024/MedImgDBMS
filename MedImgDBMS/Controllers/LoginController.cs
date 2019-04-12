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
            if (ModelState.IsValid)
            {
                using (pjmedimgdbEntities db = new pjmedimgdbEntities())
                {
                    var obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName) && a.AcctPasswd.Equals(objUser.AcctPasswd)).FirstOrDefault();
                    if (obj != null)
                    {
                        Session["UserID"] = obj.AcctID.ToString();
                        Session["AcctName"] = obj.AcctLName.ToString();
                        string role = obj.user.UserRoleID.ToString();
                        Session["UserRole"] = role;
                        if (role == "1")
                        {
                            return RedirectToAction("UserDashBoard");
                        }
                        else if (role == "2")
                        {
                            return RedirectToAction("Index", "Images");
                        }
                        else if (role == "3")
                        {
                            return RedirectToAction("Index", "Images");
                        }
                    }
                }
            }
            return View(objUser);
        }

        public ActionResult UserDashBoard()
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
    }
}