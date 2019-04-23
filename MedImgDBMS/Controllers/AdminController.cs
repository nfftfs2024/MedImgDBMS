using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MedImgDBMS.Models;

namespace MedImgDBMS.Controllers
{
    public class AdminController : Controller
    {
        private pjmedimgdbEntities db = new pjmedimgdbEntities();

        // GET: Admin
        public ActionResult Index()
        {
            var images = db.images.Include(i => i.imagestatu).Include(i => i.patient).Include(i => i.reportstatu).Include(i => i.Createuser).Include(i => i.Docuser).Include(i => i.Expuser);
            return View(images.ToList());
        }

        // GET: Admin/Details/5
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

        // GET: Admin/Create
        public ActionResult Create()
        {
            ViewBag.ImgStatus = new SelectList(db.imagestatus, "ImgStatID", "ImgStatusName");
            ViewBag.ImgPatID = new SelectList(db.patients, "PatID", "PatLName");
            ViewBag.RepStatus = new SelectList(db.reportstatus, "RepStatID", "RepStatusName");
            ViewBag.ImgCreator = new SelectList(db.users, "UserID", "UserLName");
            ViewBag.ImgDocID = new SelectList(db.users, "UserID", "UserLName");
            ViewBag.ImgExpID = new SelectList(db.users, "UserID", "UserLName");
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ImgID,ImgPath,ImgName,ImgCreateTime,ImgCreator,ImgExpID,ImgDocID,ImgPatID,ImgStatus,RepStatus")] image image)
        {
            if (ModelState.IsValid)
            {
                db.images.Add(image);
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
