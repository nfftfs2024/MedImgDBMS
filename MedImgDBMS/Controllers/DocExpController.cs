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
    public class DocExpController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: DocExp
        public ViewResult Index(string preColumn, string searchString, string statusString, string sortOrder, string currentFilter, int? page)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            List<SelectListItem> searchCol = new List<SelectListItem>()     // Set columns that can be searched by
            {
               new SelectListItem{Text="Image Name", Value = "1"},
               new SelectListItem{Text="Patient Name", Value = "2"},
               new SelectListItem{Text="Image Status", Value = "3"}
            };

            var statLst = new List<string>();   // Create a new list for image status
            statLst.Add("All");                 // Add all for status selection
            var statQry = (from i in db.images
                           where (i.ImgDocID == userID || i.ImgExpID == userID)
                           orderby i.imagestatu.ImgStatusName
                           select i.imagestatu.ImgStatusName).Distinct();    // Get distinct image status that a user has
            statLst.AddRange(statQry);      // Add distinct image status to the list

            ViewBag.StatList = new SelectList(statLst, currentFilter);              // Set image status dropdown list in viewbag
            ViewBag.SearchCol = new SelectList(searchCol, "Value", "Text");         // Set search column dropdown list in viewbag
            ViewBag.PreColumn = String.IsNullOrEmpty(preColumn) ? "1" : preColumn;  // Set previously searched column in viewbag, default at 1
            ViewBag.CurrentSort = sortOrder;        // Get current sorting
            ViewBag.TimeSortParm = String.IsNullOrEmpty(sortOrder) ? "create_time" : "";              // Sort by image create time
            ViewBag.PatFSortParm = sortOrder == "patient_first" ? "patient_first_desc" : "patient_first";   // Sort by patient first name
            ViewBag.PatLSortParm = sortOrder == "patient_last" ? "patient_last_desc" : "patient_last_time"; // Sort by patient last name
            ViewBag.StatSortParm = sortOrder == "img_status" ? "img_status_desc" : "img_status";            // Sort by image status
            ViewBag.userName = (from usr in db.users
                                where (usr.UserID == userID)
                                select usr.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            if (preColumn == "3")
                searchString = statusString;    // When last search was using the image status column, put status string into search string

            if (searchString != null)
                page = 1;                       // Conditions when users change sorting or filtering
            else
                searchString = currentFilter;   // Conditions to keep filtering string when user switches pages

            ViewBag.CurrentFilter = searchString;   // Pass current filtering string
            ViewBag.Page = (page ?? 1);                    // Pass page

            if (preColumn == "3" && searchString == "All")      // Convert filtering string to null when filtering with "All" for image status
                searchString = "";

            var images = from img in db.images  
                         where (img.ImgDocID == userID || img.ImgExpID == userID)
                         select img;                                                            // LINQ to select only user viewable images

            if (!String.IsNullOrEmpty(searchString))     // Check if search string is both empty         
            {
                int col = int.Parse(preColumn);         // Get selected search column                                                
                switch (col)
                {
                    case 1:     // When image name is selected
                        images = images.Where(s => s.ImgName.Contains(searchString));
                        break;
                    case 2:     // When patient name is selected
                        images = images.Where(s => s.patient.PatLName.Contains(searchString) || s.patient.PatFName.Contains(searchString));
                        break;
                    case 3:     // When image status is selected
                        images = images.Where(s => s.imagestatu.ImgStatusName == (searchString));
                        break;
                    default:
                        break;
                }
            }

            switch (sortOrder)      // Check sorting case
            {
                case "patient_last":        // By patient last name ascending
                    images = images.OrderBy(s => s.patient.PatLName);
                    break;
                case "patient_last_desc":   // By patient last name descending
                    images = images.OrderByDescending(s => s.patient.PatLName);
                    break;
                case "patient_first":       // By patient first name ascending
                    images = images.OrderBy(s => s.patient.PatFName);
                    break;
                case "patient_first_desc":  // By patient first name descending
                    images = images.OrderByDescending(s => s.patient.PatFName);
                    break;
                case "create_time":         // By image create time ascending
                    images = images.OrderBy(s => s.ImgCreateTime);
                    break;
                case "img_status":          // By image status ascending
                    images = images.OrderBy(s => s.ImgStatus);
                    break;
                case "img_status_desc":     // By image status descending
                    images = images.OrderByDescending(s => s.ImgStatus);
                    break;
                default:                    // Default sorting by image create time descending
                    images = images.OrderByDescending(s => s.ImgCreateTime);
                    break;
            }

            if (images == null)                 // Condition for viewing empty image list
                return View();
            else                                // Condition for viewing image list
            {
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(images.ToPagedList(pageNumber, pageSize));
            }
            //DEFAULT
            //var images = db.images.Include(i => i.patient).Include(i => i.imagestatu).Include(i => i.user).Include(i => i.user1).Include(i => i.user2);
        }

        // Doctor image view page
        public ActionResult DocImageView(long? id, int? page, string sortOrder, string currentFilter, string preColumn, string sucMsg)
        {
            image img = db.images.Find(id);                 // Find images belong to user id in DB
            report rep = (from r in db.reports
                          where r.ImgID == id
                          select r).FirstOrDefault();       // Find report belong to the image
            comment cmt = (from s in db.comments
                           where s.ImgID == id
                           select s).FirstOrDefault();      // Find comment belong to the image

            var view = new ImgRepCmtViewModels()               // Initialise a view model for passing into view
            {
                Image = img,
                Report = rep,
                Comment = cmt
            };

            //string server = db.Database.Connection.DataSource.ToString(); // Get db server name for retrieving image
            //string img_link = "http://" + server + "/" + img.ImgPath;   // Concatenate image URL
            string img_link = "~/" + img.ImgPath;

            ViewBag.link = img_link;        // Create viewbag variable for image URL
            ViewBag.Page = page;            // Create viewbag variable for current page
            ViewBag.Order = sortOrder;      // Create viewbag variable for current sort
            ViewBag.Filter = currentFilter; // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;  // Create viewbag variable for filtering column
            ViewBag.SuccessMsg = sucMsg;    // Create viewbag variable for comment successful message
            return View(view);
        }

        // Post: Doctor image view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocImageView(ImgRepCmtViewModels IRCVmodel, int id, string submit, string page, string sortOrder, string currentFilter, string preColumn)
        {
            int intPage = Convert.ToInt32(page);    // Convert page to integer
            string message = "";                    // Intialise message 

            if (IRCVmodel.Comment.CmtText is null)
            {
                message = "Comment text cannot be emtpy";        // When comment text is null
            }
            else
            {
                int userID = Convert.ToInt32(Session["UserID"].ToString());     // Get session user id
                comment cmtck = (from c in db.comments
                                 where c.ImgID == id
                                 select c).FirstOrDefault();                     // Check if it is first time to create comment for this image

                if (cmtck is null)      // When no existing comment
                {
                    image img = db.images.Find(id);                     // Find the image
                    comment cmt = new comment();                        // Create a new comment object
                    cmt.CmtText = IRCVmodel.Comment.CmtText;
                    cmt.CmtCreator = userID;
                    cmt.ImgID = id;                                     // Set comment columns

                    if (submit == "save")
                        img.ImgStatus = 11;                  // Change image status to comment uploaded
                    else
                    {
                        img.ImgStatus = 3;                   // Change image status to closed
                        img.RepStatus = 3;                   // Change report status to submitted
                    }
                    if (ModelState.IsValid)
                    {
                        db.comments.Add(cmt);
                        db.Entry(img).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Comment created";
                    }
                }
                else                    // When there is existing comment
                {
                    image img = db.images.Find(id);                     // Find the image
                    comment cmt= (from c in db.comments
                                  where c.ImgID == id
                                  select c).FirstOrDefault();           // Find the existing comment
                    cmt.CmtText = IRCVmodel.Comment.CmtText;
                    cmt.CmtCreateTime = DateTime.UtcNow;
                    cmt.CmtCreator = userID;                            // Update comment columns

                    if (submit == "save")                               // Check if the comment is saved again
                    {
                        img.ImgStatus = 11;                             // Change image status to comment uploaded
                        if (ModelState.IsValid)
                        {
                            db.Entry(cmt).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                            message = "Comment saved";
                        }
                    }
                    else                                                // The image case is closed
                    {
                        img.ImgStatus = 3;                              // Change image status to closed
                        img.RepStatus = 3;                              // Change report status to submitted
                        {
                            db.Entry(cmt).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                            message = "Image case closed";
                        }
                    }
                }
            }
            return RedirectToAction("DocImageView", new { id, sucMsg = message, page = intPage, sortOrder = sortOrder, currentFilter = currentFilter, preColumn = preColumn });    // Reload page
        }

        // Expert image view page
        public ActionResult ExpImageView(long? id, int? page, string sortOrder, string currentFilter, string preColumn, string sucMsg)
        {
            image img = db.images.Find(id);                // Find images belong to user id in DB
            report rep = (from r in db.reports
                           where r.ImgID == id
                           select r).FirstOrDefault();    // Find reports belong to this image
            comment cmt = (from s in db.comments
                           where s.ImgID == id
                           select s).FirstOrDefault();      // Find comment belong to the image

            var view = new ImgRepCmtViewModels()               // Initialise a view model for passing into view
            {
                Image = img,
                Report = rep,
                Comment = cmt
            };

            //string server = db.Database.Connection.DataSource.ToString(); // Get db server name for retrieving image
            //string img_link = "http://" + server + "/" + img.ImgPath;    // Concatenate image URL
            string img_link = "~/" + img.ImgPath;

            ViewBag.link = img_link;        // Create viewbag variable for image URL
            ViewBag.Page = page;            // Create viewbag variable for current page
            ViewBag.Order = sortOrder;      // Create viewbag variable for current sort
            ViewBag.Filter = currentFilter; // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;  // Create viewbag variable for filtering column
            ViewBag.SuccessMsg = sucMsg;    // Create viewbag variable for comment successful message
            return View(view);
        }

        // Post: Expert image view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExpImageView(ImgRepCmtViewModels IRCVmodel, int id, string submit, string page, string sortOrder, string currentFilter, string preColumn)
        {
            int intPage = Convert.ToInt32(page);    // Convert page to integer
            string message = "";                    // Intialise message 

            if (IRCVmodel.Report.RepText is null)
            {
                message = "Report text cannot be emtpy";        // When report text is null
            }
            else
            {
                int userID = Convert.ToInt32(Session["UserID"].ToString());     // Get session user id
                report repck = (from r in db.reports
                                where r.ImgID == id
                                select r).FirstOrDefault();                     // Check if it is first time to create report for this image

                if (repck is null)      // When no existing report
                {
                    image img = db.images.Find(id);                     // Find the image
                    report rep = new report();                          // Create a new report object
                    rep.RepText = IRCVmodel.Report.RepText;
                    rep.RepCreator = userID;
                    rep.ImgID = id;                                     // Set report columns

                    if (submit == "save")
                    {
                        img.ImgStatus = 10;         // Change image status to report drafted
                        img.RepStatus = 2;          // Change report status to saved
                    }
                    else
                    {
                        img.ImgStatus = 2;          // Change image status to report finalised
                        img.RepStatus = 3;          // Change report status to submitted
                    }

                    if (ModelState.IsValid)
                    {
                        db.reports.Add(rep);
                        db.Entry(img).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Report created";
                    }
                }
                else                    // When there is existing report
                {
                    image img = db.images.Find(id);                     // Find the image
                    report rep = (from r in db.reports
                                  where r.ImgID == id
                                  select r).FirstOrDefault();           // Find the existing report
                    rep.RepText = IRCVmodel.Report.RepText;
                    rep.RepCreateTime = DateTime.UtcNow;
                    rep.RepCreator = userID;                            // Update report columns

                    if (submit == "save")                               // Check if the report is saved again
                    {
                        img.ImgStatus = 10;                             // Change image status to report drafted
                        if (ModelState.IsValid)
                        {
                            db.Entry(rep).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                            message = "Report saved";
                        }
                    }
                    else                                                // The report is submitted
                    {
                        img.ImgStatus = 2;                              // Change image status to report finalised
                        img.RepStatus = 3;                              // Change report status to submitted
                        if (ModelState.IsValid)
                        {
                            db.Entry(rep).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                            message = "Report submitted";
                        }

                    }
                }
            }
            return RedirectToAction("ExpImageView", new { id, sucMsg = message, page = intPage, sortOrder = sortOrder, currentFilter = currentFilter, preColumn = preColumn });    // Reload page
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
