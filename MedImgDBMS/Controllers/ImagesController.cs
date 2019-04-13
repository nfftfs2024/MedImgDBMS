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
    public class ImagesController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: Images
        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Session["UserID"] != null ? Session["UserID"].ToString() : "0");   // Convert session user id to integer for comparison and prevent from NULL
            var images = from img in db.images  
                         where (img.ImgDocID == userID || img.ImgExpID == userID)
                         select img;                                                                        // LINQ to select only user viewable images

            if (images == null)                 // Condition for viewing empty image list
                return View();
            else                                // Condition for viewing image list
                return View(images.ToList());

            //DEFAULT
            //var images = db.images.Include(i => i.patient).Include(i => i.imagestatu).Include(i => i.user).Include(i => i.user1).Include(i => i.user2);
        }

        // GET: Images/Details/5
        public ActionResult Details(long? id)
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
            return View(image);
        }

        // GET: Images/Create
        public ActionResult Create()
        {
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName");
            ViewBag.StatusID = new SelectList(db.imagestatus, "ImgStatID", "StatusName");
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName");
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName");
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName");
            return View();
        }

        // POST: Images/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImgID,ImgPath,ImgName,ImgCreateTime,ImgCreator,ImgExpID,ImgDocID,ImgPatID,StatusID")] image image)
        {
            if (ModelState.IsValid)
            {
                db.images.Add(image);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName", image.ImgPatID);
            ViewBag.StatusID = new SelectList(db.imagestatus, "ImgStatID", "StatusName", image.StatusID);
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName", image.ImgCreator);
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName", image.ImgDocID);
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName", image.ImgExpID);
            return View(image);
        }

        // GET: Images/Edit/5
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
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName", image.ImgPatID);
            ViewBag.StatusID = new SelectList(db.imagestatus, "ImgStatID", "StatusName", image.StatusID);
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName", image.ImgCreator);
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName", image.ImgDocID);
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName", image.ImgExpID);
            return View(image);
        }

        // POST: Images/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ImgID,ImgPath,ImgName,ImgCreateTime,ImgCreator,ImgExpID,ImgDocID,ImgPatID,StatusID")] image image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName", image.ImgPatID);
            ViewBag.StatusID = new SelectList(db.imagestatus, "ImgStatID", "StatusName", image.StatusID);
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName", image.ImgCreator);
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName", image.ImgDocID);
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName", image.ImgExpID);
            return View(image);
        }

        // GET: Images/Delete/5
        public ActionResult Delete(long? id)
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
            return View(image);
        }

        // POST: Images/Delete/5
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
