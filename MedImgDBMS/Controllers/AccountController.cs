﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MedImgDBMS.Models;
using System.Web.Security;

namespace MedImgDBMS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(account objUser)
        {
            string message = string.Empty;      // Create empty string

            if (ModelState.IsValid)
            {
                using (pjmedimgdbEntities db = new pjmedimgdbEntities())
                {
                    if (objUser.AcctLName != null && objUser.AcctPasswd != null)        // Check if user didn't enter anything
                    { 
                        var obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName)).FirstOrDefault();    // Check if account name exists in DB
                        if (obj != null)
                        {
                            obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName) && a.AcctPasswd.Equals(objUser.AcctPasswd)).FirstOrDefault();    // Compare account name and passwd with DB
                            if (obj != null)
                            {
                                //FormsAuthentication.SetAuthCookie(obj.AcctLName, true);     // Set authentication cookie with account name
                                var now = DateTime.Now;
                                string roles = obj.user.role.RoleName.ToString();
                                var ticket = new FormsAuthenticationTicket(
                                    version: 1,
                                    name: objUser.AcctID.ToString(),
                                    issueDate: now,
                                    expiration: now.AddMinutes(30),
                                    isPersistent: true,
                                    userData: roles,
                                    cookiePath: FormsAuthentication.FormsCookiePath);
                                var encryptedticket = FormsAuthentication.Encrypt(ticket);
                                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedticket);
                                Response.Cookies.Add(cookie);
                                Session["UserID"] = obj.AcctID.ToString();          // Get session user id
                                Session["AcctName"] = obj.AcctLName.ToString();     // Get session account name

                                string role = obj.user.UserRoleID.ToString();
                                Session["UserRole"] = role;                         // Get session user role

                                switch (role)                                       // Redirect page according to roles
                                {
                                    case "1":
                                        return RedirectToAction("Index", "Admin");
                                    case "2":
                                        return RedirectToAction("Index", "DocExp", new { preColumn = "3", statusString = "New" });
                                    case "3":
                                        return RedirectToAction("Index", "DocExp", new { preColumn = "3", statusString = "New" });
                                }
                            }
                            else
                            {
                                message = "Password is incorrect!!";
                            }
                        }
                        else
                        {
                            message = "Account does not exist!!";
                        }
                    }
                    else
                        message = "Enter your account and password";
                }
            }
            ViewBag.Message = message;
            return View(objUser);
        }

        public ActionResult Logout()                    // For system logout
        {
            FormsAuthentication.SignOut();      
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}