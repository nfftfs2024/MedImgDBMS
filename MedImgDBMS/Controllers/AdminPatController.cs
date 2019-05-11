using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MedImgDBMS.Models;
using PagedList;

namespace MedImgDBMS.Controllers
{
    public class AdminPatController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: Admin
        public ViewResult Index(string preColumn, string searchString, string sortOrder, string currentFilter, int? page, string sucMsg)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            List<SelectListItem> searchCol = new List<SelectListItem>()     // Set columns that can be searched by
            {
               new SelectListItem{Text="Patient ID", Value = "1"},
               new SelectListItem{Text="Patient Name", Value = "2"},
            };

            ViewBag.SearchCol = new SelectList(searchCol, "Value", "Text");         // Set search column dropdown list in viewbag
            ViewBag.PreColumn = String.IsNullOrEmpty(preColumn) ? "1" : preColumn;  // Set previously searched column in viewbag, default at 1
            ViewBag.SuccessMsg = sucMsg;            // Get successful message
            ViewBag.CurrentSort = sortOrder;        // Get current sorting
            ViewBag.IDSortParm = String.IsNullOrEmpty(sortOrder) ? "pid_desc" : "";                         // Sort by patient id 
            ViewBag.PatFSortParm = sortOrder == "patient_first" ? "patient_first_desc" : "patient_first";   // Sort by patient first name
            ViewBag.PatLSortParm = sortOrder == "patient_last" ? "patient_last_desc" : "patient_last";      // Sort by patient last name
            ViewBag.DOBSortParm = sortOrder == "dob" ? "dob_desc" : "dob";                                  // Sort by DOB
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            if (searchString != null)
                page = 1;                       // Conditions when users change sorting or filtering
            else
                searchString = currentFilter;   // Conditions to keep filtering string when user switches pages

            ViewBag.CurrentFilter = searchString;   // Pass current filtering string
            ViewBag.Page = (page ?? 1);             // Pass page

            var pat = from p in db.patients
                      select p;                     // LINQ to select patients

            if (!String.IsNullOrEmpty(searchString))     // Check if search string is both empty         
            {
                int col = int.Parse(preColumn);         // Get selected search column                                                
                switch (col)
                {
                    case 1:     // When patient id is selected
                        pat = pat.Where(p => p.PatID.ToString().Contains(searchString));
                        break;
                    case 2:     // When patient name is selected
                        pat = pat.Where(p => p.PatLName.Contains(searchString) || p.PatFName.Contains(searchString));
                        break;
                    default:
                        break;
                }
            }

            switch (sortOrder)      // Check sorting case
            {
                case "pid_desc":            // By patient id descending
                    pat = pat.OrderByDescending(p => p.PatID);
                    break;
                case "patient_first":       // By patient first name ascending
                    pat = pat.OrderBy(p => p.PatFName);
                    break;
                case "patient_first_desc":  // By patient first name descending
                    pat = pat.OrderByDescending(p => p.PatFName);
                    break;
                case "patient_last":        // By patient last name ascending
                    pat = pat.OrderBy(p => p.PatLName);
                    break;
                case "patient_last_desc":   // By patient last name descending
                    pat = pat.OrderByDescending(p => p.PatLName);
                    break;
                case "dob":                 // By dob ascending
                    pat = pat.OrderBy(p => p.DOB);
                    break;
                case "dob_desc":            // By dob descending
                    pat = pat.OrderByDescending(p => p.DOB);
                    break;
                default:                    // Default sorting by patient id ascending
                    pat = pat.OrderBy(p => p.PatID);
                    break;
            }

            if (pat == null)                    // Condition for viewing empty patient list
                return View();
            else                                // Condition for viewing patient list
            {
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(pat.ToPagedList(pageNumber, pageSize));
            }
            //DEFAULT
            //var images = db.images.Include(i => i.patient).Include(i => i.imagestatu).Include(i => i.user).Include(i => i.user1).Include(i => i.user2);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            List<SelectListItem> gender = new List<SelectListItem>()     // Set columns for gender selection
            {
               new SelectListItem{Text="Male", Value = "M"},
               new SelectListItem{Text="Female", Value = "F"},
            };

            ViewBag.Gender = new SelectList(gender, "Value", "Text");         // Set gender dropdown list in viewbag
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            patient pat = new patient();          // Create a new patient
            return View(pat);
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(patient patient, string Gender)
        {
            int userID = Convert.ToInt32(Session["UserID"].ToString());     // Get session user id
            patient.Gender = Gender;

            if (ModelState.IsValid)         // Add patient
            {
                db.patients.Add(patient);
                db.SaveChanges();
                return RedirectToAction("Index", new { sucMsg = "New patient created" });     // Go back to list and display successful message
            }

            // If the model was invalid, display and do it over again
            List<SelectListItem> gender = new List<SelectListItem>()     // Set columns for gender selection
            {
               new SelectListItem{Text="Male", Value = "M"},
               new SelectListItem{Text="Female", Value = "F"},
            };

            ViewBag.Gender = new SelectList(gender, "Value", "Text");         // Set gender dropdown list in viewbag
            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            patient pat = new patient();          // Create a new patient
            return View(pat);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(long? id, int? page, string sortOrder, string currentFilter, string preColumn)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            patient pat = db.patients.Find(id);                     // Find patient

            if (pat == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> gender = new List<SelectListItem>()     // Set columns for gender selection
            {
               new SelectListItem{Text="Male", Value = "M"},
               new SelectListItem{Text="Female", Value = "F"},
            };

            ViewBag.Gen = new SelectList(gender, "Value", "Text", pat.Gender);         // Set gender dropdown list in viewbag
            ViewBag.Page = page;                    // Create viewbag variable for current page
            ViewBag.CurrentSort = sortOrder;          // Create viewbag variable for current sort
            ViewBag.CurrentFilter = currentFilter;  // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;          // Create viewbag variable for filtering column

            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            return View(pat);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string page, string sortOrder, string currentFilter, string preColumn, patient patient)
        {
            int intPage = Convert.ToInt32(page);    // Convert page to integer

            if (ModelState.IsValid)     // Update patient
            {
                db.Entry(patient).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { sucMsg = "Patient modified", page = intPage, sortOrder = sortOrder, currentFilter = currentFilter, preColumn = preColumn });    // Go back to list and display successful message
            }

            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            ViewBag.Page = intPage;                 // Create viewbag variable for current page
            ViewBag.CurrentSort = sortOrder;          // Create viewbag variable for current sort
            ViewBag.CurrentFilter = currentFilter;  // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;          // Create viewbag variable for filtering column

            ViewBag.userName = (from u in db.users
                                where (u.UserID == userID)
                                select u.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            return View(patient);
        }

        public ActionResult Delete(long? id)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            patient pat = db.patients.Find(id);

            ViewBag.userName = (from usr in db.users
                                where (usr.UserID == userID)
                                select usr.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            if (pat == null)
            {
                return HttpNotFound();
            }
            return View(pat);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            patient pat = db.patients.Find(id);
            db.patients.Remove(pat);
            db.SaveChanges();
            return RedirectToAction("Index");
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
