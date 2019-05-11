using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MedImgDBMS.Models;
using MedImgDBMS.ViewModels;
using PagedList;

namespace MedImgDBMS.Controllers
{
    public class AdminUserController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: Admin
        public ViewResult Index(string preColumn, string searchString, string sortOrder, string currentFilter, int? page, string sucMsg)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            List<SelectListItem> searchCol = new List<SelectListItem>()     // Set columns that can be searched by
            {
               new SelectListItem{Text="User ID", Value = "1"},
               new SelectListItem{Text="User Name", Value = "2"},
               new SelectListItem{Text="Email", Value = "3"}
            };

            ViewBag.SearchCol = new SelectList(searchCol, "Value", "Text");         // Set search column dropdown list in viewbag
            ViewBag.PreColumn = String.IsNullOrEmpty(preColumn) ? "1" : preColumn;  // Set previously searched column in viewbag, default at 1
            ViewBag.SuccessMsg = sucMsg;            // Get successful message
            ViewBag.CurrentSort = sortOrder;        // Get current sorting
            ViewBag.IDSortParm = String.IsNullOrEmpty(sortOrder) ? "uid_desc" : "";                 // Sort by user id 
            ViewBag.UsrFSortParm = sortOrder == "user_first" ? "user_first_desc" : "user_first";    // Sort by user first name
            ViewBag.UsrLSortParm = sortOrder == "user_last" ? "user_last_desc" : "user_last";       // Sort by user last name
            ViewBag.MailSortParm = sortOrder == "mail" ? "mail_desc" : "mail";                      // Sort by email
            ViewBag.RoleSortParm = sortOrder == "role" ? "role_desc" : "role";                      // Sort by role
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            if (searchString != null)
                page = 1;                       // Conditions when users change sorting or filtering
            else
                searchString = currentFilter;   // Conditions to keep filtering string when user switches pages

            ViewBag.CurrentFilter = searchString;   // Pass current filtering string
            ViewBag.Page = (page ?? 1);             // Pass page

            var usr = from u in db.users
                      select u;                     // LINQ to select users

            if (!String.IsNullOrEmpty(searchString))     // Check if search string is both empty         
            {
                int col = int.Parse(preColumn);         // Get selected search column                                                
                switch (col)
                {
                    case 1:     // When user id is selected
                        usr = usr.Where(u => u.UserID.ToString().Contains(searchString));
                        break;
                    case 2:     // When user name is selected
                        usr = usr.Where(u => u.UserLName.Contains(searchString) || u.UserFName.Contains(searchString));
                        break;
                    case 3:     // When email is selected
                        usr = usr.Where(u => u.Email.Contains(searchString));
                        break;
                    default:
                        break;
                }
            }

            switch (sortOrder)      // Check sorting case
            {
                case "uid_desc":            // By user id descending
                    usr = usr.OrderByDescending(u => u.UserID);
                    break;
                case "user_first":          // By user first name ascending
                    usr = usr.OrderBy(u => u.UserFName);
                    break;
                case "user_first_desc":     // By user first name descending
                    usr = usr.OrderByDescending(u => u.UserFName);
                    break;
                case "user_last":           // By user last name ascending
                    usr = usr.OrderBy(u => u.UserLName);
                    break;
                case "user_last_desc":      // By user last name descending
                    usr = usr.OrderByDescending(u => u.UserLName);
                    break;
                case "mail":                // By email ascending
                    usr = usr.OrderBy(u => u.Email);
                    break;
                case "mail_desc":           // By email descending
                    usr = usr.OrderByDescending(u => u.Email);
                    break;
                case "role":                // By email ascending
                    usr = usr.OrderBy(u => u.role.RoleName);
                    break;
                case "role_desc":           // By email descending
                    usr = usr.OrderByDescending(u => u.role.RoleName);
                    break;
                default:                    // Default sorting by user id ascending
                    usr = usr.OrderBy(u => u.UserID);
                    break;
            }

            if (usr == null)                    // Condition for viewing empty user list
                return View();
            else                                // Condition for viewing user list
            {
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(usr.ToPagedList(pageNumber, pageSize));
            }
            //DEFAULT
            //var images = db.images.Include(i => i.patient).Include(i => i.imagestatu).Include(i => i.user).Include(i => i.user1).Include(i => i.user2);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            var usr = from u in db.users
                      select u.UserID;                                              // Get current user IDs in database
            TempData["usrMax"] = Convert.ToInt32(usr.Max().ToString()) + 1;         // Put next user id into temp data

            var Role = db.roles.Select(r => new SelectListItem               // Create role selection list
            {
                Text = r.RoleName,
                Value = r.RoleID.ToString()
            });

            ViewBag.UserRoleID = new SelectList(Role.OrderBy(r => r.Text), "Value", "Text");      // Pass role selection list

            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            UserAcctViewModels uaModel = new UserAcctViewModels();          // Create a new user account view model
            return View(uaModel);
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserAcctViewModels uaModel, string UserRoleID)
        {
            int userID = Convert.ToInt32(Session["UserID"].ToString());                                                     // Get session user id
            uaModel.User.UserRoleID = Convert.ToInt32(UserRoleID != null ? UserRoleID.ToString() : "1");                    // Set user role id
            int acctID = Convert.ToInt32(TempData["usrMax"] != null ? TempData["usrMax"].ToString() : "1000");              // Set account id

            account acct = new account()    // Create a new account
            {
                AcctID = acctID,
                AcctLName = uaModel.AcctLName,
                AcctPasswd = uaModel.AcctPasswd
            };

            if (ModelState.IsValid)         // Add user and account
            {
                db.users.Add(uaModel.User);
                db.SaveChanges();
                db.accounts.Add(acct);
                db.SaveChanges();
                return RedirectToAction("Index", new { sucMsg = "New user created" });     // Go back to list and display successful message
            }

            // If the model was invalid, display and do it over again
            var usr = from u in db.users
                      select u.UserID;                                              // Get current user IDs in database
            TempData["usrMax"] = Convert.ToInt32(usr.Max().ToString()) + 1;         // Put next user id into temp data

            var Role = db.roles.Select(r => new SelectListItem              // Create role selection list
            {
                Text = r.RoleName,
                Value = r.RoleID.ToString()
            });

            ViewBag.UserRoleID = new SelectList(Role.OrderBy(r => r.Text), "Value", "Text");      // Pass role selection list

            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            UserAcctViewModels uaModelA = new UserAcctViewModels();          // Create a new user account view model
            return View(uaModelA);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(long? id, int? page, string sortOrder, string currentFilter, string preColumn)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            user usr = db.users.Find(id);                           // Find user
            account acct = db.accounts.Find(id);                    // Find account
            var view = new UserAcctEditViewModels()                 // Initialise a view model for passing into view
            {
                User = usr,
                Account = acct
            };

            if (usr == null)
            {
                return HttpNotFound();
            }

            ViewBag.Page = page;                        // Create viewbag variable for current page
            ViewBag.CurrentSort = sortOrder;              // Create viewbag variable for current sort
            ViewBag.CurrentFilter = currentFilter;      // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;              // Create viewbag variable for filtering column
            ViewBag.UserRoleID = new SelectList(db.roles, "RoleID", "RoleName", usr.UserRoleID);                         // Pass role selection list with default value
            ViewBag.AcctStatus = new SelectList(db.accountstatus, "AcctStatID", "AcctStatusName", acct.AcctStatus);      // Pass account status selection list with default value
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            return View(view);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string page, string sortOrder, string currentFilter, string preColumn, UserAcctEditViewModels uaeModel, string UserRoleID, string AcctStatus)
        {
            int intPage = Convert.ToInt32(page);    // Convert page to integer

            uaeModel.User.UserRoleID = Convert.ToInt32(UserRoleID != null ? UserRoleID.ToString() : "1");                    // Set user role id
            uaeModel.Account.AcctStatus = Convert.ToInt32(AcctStatus != null ? AcctStatus.ToString() : "1");                 // Set account status

            if (ModelState.IsValid)     // Update user and account
            {
                db.Entry(uaeModel.User).State = EntityState.Modified;
                db.Entry(uaeModel.Account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { sucMsg = "User modified", page = intPage, sortOrder = sortOrder, currentFilter = currentFilter, preColumn = preColumn });    // Go back to list and display successful message
            }

            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            ViewBag.Page = intPage;                 // Create viewbag variable for current page
            ViewBag.CurrentSort = sortOrder;          // Create viewbag variable for current sort
            ViewBag.CurrentFilter = currentFilter;  // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;          // Create viewbag variable for filtering column
            ViewBag.UserRoleID = new SelectList(db.roles, "RoleID", "RoleName", uaeModel.User.UserRoleID);                              // Pass role selection list with default value
            ViewBag.AcctStatus = new SelectList(db.accountstatus, "AcctStatID", "AcctStatusName", uaeModel.Account.AcctStatus);      // Pass account status selection list with default value
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            return View(uaeModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
