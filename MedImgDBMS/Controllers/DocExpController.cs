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

namespace MedImgDBMS.Controllers
{
    public class DocExpController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: DocExp
        public ActionResult Index(string preColumn, string searchString, string statusString)
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL

            List<SelectListItem> SearchCol = new List<SelectListItem>()     // Set columns that can be searched by
            {
               new SelectListItem{Text="Image Name", Value = "1"},
               new SelectListItem{Text="Patient Name", Value = "2"},
               new SelectListItem{Text="Image Status", Value = "3"}
            };

            var StatLst = new List<string>();                   // Create a new list for image status
            var StatQry = (from i in db.images
                           where (i.ImgDocID == userID || i.ImgExpID == userID)
                           orderby i.imagestatu.ImgStatusName
                           select i.imagestatu.ImgStatusName).Distinct();    // Get distinct image status that a user has
            StatLst.AddRange(StatQry);      // Add distinct image status to the list

            ViewBag.statList = new SelectList(StatLst);                         // Set image status dropdown list in viewbag
            ViewBag.searchCol = new SelectList(SearchCol, "Value", "Text");     // Set search column dropdown list in viewbag
            ViewBag.precolumn = "1";                                            // Set previously searched column in viewbag, default at 1

            if (preColumn == "3")
                ViewBag.preColumn = "3";    // When last search was using the image status column

            var images = from img in db.images  
                         where (img.ImgDocID == userID || img.ImgExpID == userID)
                         select img;                                                            // LINQ to select only user viewable images

            if (!String.IsNullOrEmpty(searchString) || !String.IsNullOrEmpty(statusString))     // Check if either text area or status dropdown list are both empty         
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
                        images = images.Where(s => s.imagestatu.ImgStatusName == (statusString));
                        break;
                    default:
                        break;
                }
            }

            ViewBag.userName = (from usr in db.users
                                where (usr.UserID == userID)
                                select usr.UserFName).FirstOrDefault().ToString();      // Passing user first name to view

            if (images == null)                 // Condition for viewing empty image list
                return View();
            else                                // Condition for viewing image list
                return View(images.ToList());

            //DEFAULT
            //var images = db.images.Include(i => i.patient).Include(i => i.imagestatu).Include(i => i.user).Include(i => i.user1).Include(i => i.user2);
        }

        // Doctor image view page
        public ActionResult DocImageView(long? id)
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

            string server = db.Database.Connection.DataSource.ToString(); // Get db server name for retrieving image
            string img_link = "http://" + server + "/" + img.ImgPath;   // Concatenate image URL
            ViewBag.link = img_link;                                      // Create viewbag variable for image URL
            return View(view);
        }

        // Post: Doctor image view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocImageView(ImgRepCmtViewModels IRCVmodel, int id, string submit)
        {
            if (IRCVmodel.Comment.CmtText is null)
            {
                ViewBag.Message = "Comment text cannot be emtpy";        // When comment text is null..... TODO......
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

                    img.ImgStatus = img.ImgStatus + 1;                  // Change image status to comment uploaded

                    if (ModelState.IsValid)
                    {
                        db.comments.Add(cmt);
                        db.Entry(img).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else                    // When there is existing comment
                {
                    comment cmt= (from c in db.comments
                                  where c.ImgID == id
                                  select c).FirstOrDefault();           // Find the existing comment
                    cmt.CmtText = IRCVmodel.Comment.CmtText;
                    cmt.CmtCreateTime = DateTime.UtcNow;
                    cmt.CmtCreator = userID;                            // Update comment columns

                    if (submit == "save")                               // Check if the comment is saved again
                    {
                        if (ModelState.IsValid)
                        {
                            db.Entry(cmt).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else                                                // The image case is closed
                    {
                        image img = db.images.Find(id);
                        img.ImgStatus = img.ImgStatus + 1;              // Change image status to closed
                        {
                            db.Entry(cmt).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            return RedirectToAction("DocImageView", id);    // Reload page
        }

        // Expert image view page
        public ActionResult ExpImageView(long? id)
        {
            image img = db.images.Find(id);                // Find images belong to user id in DB
            report rep = (from r in db.reports
                           where r.ImgID == id
                           select r).FirstOrDefault();    // Find reports belong to this image

            var view = new ImgRepCmtViewModels()               // Initialise a view model for passing into view
            {
                Image = img,
                Report = rep
            };

            string server = db.Database.Connection.DataSource.ToString(); // Get db server name for retrieving image
            string img_link = "http://" + server + "/" + img.ImgPath;    // Concatenate image URL
            ViewBag.link = img_link;                                      // Create viewbag variable for image URL
            return View(view);
        }

        // Post: Expert image view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExpImageView(ImgRepCmtViewModels IRCVmodel, int id, string submit)
        {
            if (IRCVmodel.Report.RepText is null)
            {
                ViewBag.Message = "Report text cannot be emtpy";        // When report text is null..... TODO......
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

                    img.ImgStatus = img.ImgStatus + 1;                  // Change image status to report drafted

                    if (ModelState.IsValid)
                    {
                        db.reports.Add(rep);
                        db.Entry(img).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else                    // When there is existing report
                {
                    report rep = (from r in db.reports
                                  where r.ImgID == id
                                  select r).FirstOrDefault();           // Find the existing report
                    rep.RepText = IRCVmodel.Report.RepText;
                    rep.RepCreateTime = DateTime.UtcNow;
                    rep.RepCreator = userID;                            // Update report columns

                    if (submit == "save")                               // Check if the report is saved again
                    {
                        if (ModelState.IsValid)
                        {
                            db.Entry(rep).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else                                                // The report is submitted
                    {
                        image img = db.images.Find(id);
                        img.ImgStatus = img.ImgStatus + 1;              // Change image status to report finalised
                        if (ModelState.IsValid)
                        {
                            db.Entry(rep).State = EntityState.Modified;
                            db.Entry(img).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                }
            }
            return RedirectToAction("ExpImageView", id);    // Reload page
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
