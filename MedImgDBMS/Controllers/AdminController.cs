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
    public class AdminController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: Admin
        public ViewResult Index(string preColumn, string searchString, string statusString, string sortOrder, string currentFilter, int? page, string sucMsg)
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
                           orderby i.imagestatu.ImgStatusName
                           select i.imagestatu.ImgStatusName).Distinct();    // Get distinct image status that a user has
            statLst.AddRange(statQry);      // Add distinct image status to the list

            ViewBag.StatList = new SelectList(statLst);                             // Set image status dropdown list in viewbag
            ViewBag.SearchCol = new SelectList(searchCol, "Value", "Text");         // Set search column dropdown list in viewbag
            ViewBag.PreColumn = String.IsNullOrEmpty(preColumn) ? "1" : preColumn;  // Set previously searched column in viewbag, default at 1
            ViewBag.SuccessMsg = sucMsg;            // Get successful message
            ViewBag.CurrentSort = sortOrder;        // Get current sorting
            ViewBag.PatLSortParm = String.IsNullOrEmpty(sortOrder) ? "patient_last_desc" : "";              // Sort by patient last name
            ViewBag.PatFSortParm = sortOrder == "patient_first" ? "patient_first_desc" : "patient_first";   // Sort by patient first name
            ViewBag.TimeSortParm = sortOrder == "create_time" ? "create_time_desc" : "create_time";         // Sort by image create time
            ViewBag.StatSortParm = sortOrder == "img_status" ? "img_status_desc" : "img_status";            // Sort by image status
            ViewBag.ImgNameSortParm = sortOrder == "img_name" ? "img_name_desc" : "img_name";               // Sort by image name
            ViewBag.CreatorSortParm = sortOrder == "creator" ? "creator_desc" : "creator";                  // Sort by image creator
            ViewBag.DocSortParm = sortOrder == "doctor" ? "doctor_desc" : "doctor";                         // Sort by image doctor
            ViewBag.ExpSortParm = sortOrder == "expert" ? "expert_desc" : "expert";                         // Sort by image expert
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
                         select img;                     // LINQ to select viewable images

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
                case "create_time_desc":    // By image create time descending
                    images = images.OrderByDescending(s => s.ImgCreateTime);
                    break;
                case "img_status":          // By image status ascending
                    images = images.OrderBy(s => s.ImgStatus);
                    break;
                case "img_status_desc":     // By image status descending
                    images = images.OrderByDescending(s => s.ImgStatus);
                    break;
                case "img_name":
                    images = images.OrderBy(s => s.ImgName);
                    break;
                case "img_name_desc":
                    images = images.OrderByDescending(s => s.ImgName);
                    break;
                case "creator":
                    images = images.OrderBy(s => s.Createuser.UserFName);
                    break;
                case "creator_desc":
                    images = images.OrderByDescending(s => s.Createuser.UserFName);
                    break;
                case "doctor":
                    images = images.OrderBy(s => s.Docuser.UserFName);
                    break;
                case "doctor_desc":
                    images = images.OrderByDescending(s => s.Docuser.UserFName);
                    break;
                case "expert":
                    images = images.OrderBy(s => s.Expuser.UserFName);
                    break;
                case "expert_desc":
                    images = images.OrderByDescending(s => s.Expuser.UserFName);
                    break;
                default:                    // Default sorting by patient last name ascending
                    images = images.OrderBy(s => s.patient.PatLName);
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

        // GET: Admin image view
        public ActionResult ImageView(long? id, int? page, string sortOrder, string currentFilter, string preColumn)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            image image = db.images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }

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

            ViewBag.link = img_link;        // Create viewbag variable for image URL
            ViewBag.Page = page;            // Create viewbag variable for current page
            ViewBag.Order = sortOrder;      // Create viewbag variable for current sort
            ViewBag.Filter = currentFilter; // Create viewbag variable for current filter
            ViewBag.PreColumn = preColumn;  // Create viewbag variable for filtering column
            return View(view);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            var img = from i in db.images
                      select i.ImgID;                                       // Get current image IDs in database
            int imgMax = Convert.ToInt32(img.Max().ToString()) + 1;         // Get the next image ID
            string imgName = "image_" + imgMax.ToString().PadLeft(6, '0');  // Create image name and pad the image number

            var Patient = db.patients.Select(p => new SelectListItem                                // Create patient selection list
            {
                Text = p.PatFName + " " + p.PatLName,
                Value = p.PatID.ToString()
            });
            var DocUser = db.users.Where(p => p.UserRoleID == 2).Select(p => new SelectListItem     // Create doctor selection list
            {
                Text = p.UserFName + " " + p.UserLName,
                Value = p.UserID.ToString()
            });
            var ExpUser = db.users.Where(p => p.UserRoleID == 3).Select(p => new SelectListItem     // Create expert selection list
            {
                Text = p.UserFName + " " + p.UserLName,
                Value = p.UserID.ToString()
            });

            ViewBag.ImgID = imgMax;             // Pass the image ID
            ViewBag.ImgName = imgName;          // Pass the image name
            ViewBag.ImgPatID = new SelectList(Patient.OrderBy(p => p.Text), "Value", "Text");       // Pass the patient selection list
            ViewBag.ImgDocID = new SelectList(DocUser.OrderBy(p => p.Text), "Value", "Text");       // Pass the doctor selection list
            ViewBag.ImgExpID = new SelectList(ExpUser.OrderBy(p => p.Text), "Value", "Text");       // Pass the expert selection list

            image image = new image();      // Create a new image object
            return View(image);
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(image newImg, string PatientId)
        {
            int userID = Convert.ToInt32(Session["UserID"].ToString());     // Get session user id
            newImg.ImgPath = newImg.ImgPath + newImg.ImgName + ".jpg";      // Set image path
            newImg.ImgCreator = userID;                                     // Set image creator id

            if (ModelState.IsValid)         // Add image
            {
                db.images.Add(newImg);
                db.SaveChanges();
                return RedirectToAction("Index", new { sucMsg = newImg.ImgName + " created" });     // Go back to list and display successful message
            }

            // If the model was invalid, display and do it over again
            var img = from i in db.images
                      select i.ImgID;                                       // Get current image IDs in database
            int imgMax = Convert.ToInt32(img.Max().ToString()) + 1;         // Get the next image ID
            string imgName = "image_" + imgMax.ToString().PadLeft(6, '0');  // Create image name and pad the image number

            var Patient = db.patients.Select(p => new SelectListItem                                // Create patient selection list
            {
                Text = p.PatFName + " " + p.PatLName,
                Value = p.PatID.ToString()
            });
            var DocUser = db.users.Where(p => p.UserRoleID == 2).Select(p => new SelectListItem     // Create doctor selection list
            {
                Text = p.UserFName + " " + p.UserLName,
                Value = p.UserID.ToString()
            });
            var ExpUser = db.users.Where(p => p.UserRoleID == 3).Select(p => new SelectListItem     // Create expert selection list
            {
                Text = p.UserFName + " " + p.UserLName,
                Value = p.UserID.ToString()
            });

            ViewBag.ImgID = imgMax;             // Pass the image ID
            ViewBag.ImgName = imgName;          // Pass the image name
            ViewBag.ImgPatID = new SelectList(Patient.OrderBy(p => p.Text), "Value", "Text");       // Pass the patient selection list
            ViewBag.ImgDocID = new SelectList(DocUser.OrderBy(p => p.Text), "Value", "Text");       // Pass the doctor selection list
            ViewBag.ImgExpID = new SelectList(ExpUser.OrderBy(p => p.Text), "Value", "Text");       // Pass the expert selection list

            image image = new image();      // Create a new image object
            return View(image);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            image image = db.images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            ViewBag.ImgStatus = new SelectList(db.imagestatus, "ImgStatID", "ImgStatusName", image.ImgStatus);
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName", image.ImgPatID);
            ViewBag.RepStatus = new SelectList(db.reportstatus, "RepStatID", "RepStatusName", image.RepStatus);
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName", image.ImgCreator);
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName", image.ImgDocID);
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName", image.ImgExpID);
            return View(image);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImgID,ImgPath,ImgName,ImgCreateTime,ImgCreator,ImgExpID,ImgDocID,ImgPatID,ImgStatus,RepStatus")] image image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ImgStatus = new SelectList(db.imagestatus, "ImgStatID", "ImgStatusName", image.ImgStatus);
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName", image.ImgPatID);
            ViewBag.RepStatus = new SelectList(db.reportstatus, "RepStatID", "RepStatusName", image.RepStatus);
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName", image.ImgCreator);
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName", image.ImgDocID);
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName", image.ImgExpID);
            return View(image);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            image image = db.images.Find(id);

            string server = db.Database.Connection.DataSource.ToString(); // Get db server name for retrieving image
            string img_link = "http://" + server + "/" + image.ImgPath;   // Concatenate image URL
            ViewBag.link = img_link;        // Create viewbag variable for image URL

            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            image image = db.images.Find(id);
            db.images.Remove(image);
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
