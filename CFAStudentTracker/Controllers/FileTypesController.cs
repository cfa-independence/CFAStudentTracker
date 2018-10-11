using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;

namespace CFAStudentTracker.Controllers
{
    [Authorize]
    public class FileTypesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: FileTypes
        public async Task<ActionResult> Index()
        {
            return View(await db.FileType.ToListAsync());
        }

        // GET: FileTypes/Details/5
        public async Task<ActionResult> Details(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileType fileType = await db.FileType.FindAsync(id);
            if (fileType == null)
            {
                return HttpNotFound();
            }
            return View(fileType);
        }

        // GET: FileTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FileTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FileTypeID,TypeDescription,TypeWeight")] FileType fileType)
        {
            if (ModelState.IsValid)
            {
                db.FileType.Add(fileType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(fileType);
        }

        // GET: FileTypes/Edit/5
        public async Task<ActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileType fileType = await db.FileType.FindAsync(id);
            if (fileType == null)
            {
                return HttpNotFound();
            }
            return View(fileType);
        }

        // POST: FileTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FileTypeID,TypeDescription,TypeWeight")] FileType fileType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fileType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(fileType);
        }

        // GET: FileTypes/Delete/5
        public async Task<ActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileType fileType = await db.FileType.FindAsync(id);
            if (fileType == null)
            {
                return HttpNotFound();
            }
            return View(fileType);
        }

        // POST: FileTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(short id)
        {
            FileType fileType = await db.FileType.FindAsync(id);
            db.FileType.Remove(fileType);
            await db.SaveChangesAsync();
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
