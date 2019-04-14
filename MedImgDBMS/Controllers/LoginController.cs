﻿using System;
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
                    if (objUser.AcctLName != null && objUser.AcctPasswd != null)        // Check if user didn't enter anything
                    { 
                        var obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName)).FirstOrDefault();    // Check if account name exists in DB
                        if (obj != null)
                        {
                            obj = db.accounts.Where(a => a.AcctLName.Equals(objUser.AcctLName) && a.AcctPasswd.Equals(objUser.AcctPasswd)).FirstOrDefault();    // Compare account name and passwd with DB
                            if (obj != null)
                            {
                                Session["UserID"] = obj.AcctID.ToString();          // Get session user id
                                Session["AcctName"] = obj.AcctLName.ToString();     // Get session account name

                                string role = obj.user.UserRoleID.ToString();
                                Session["UserRole"] = role;                         // Get session user role

                                switch (role)                                       // Redirect page according to roles
                                {
                                    case "1":
                                        return RedirectToAction("UserDashBoard");
                                    case "2":
                                        return RedirectToAction("Index", "Images");
                                    case "3":
                                        return RedirectToAction("Index", "Images");
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