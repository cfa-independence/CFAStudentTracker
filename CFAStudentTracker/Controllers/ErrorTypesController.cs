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
    public class ErrorTypesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: ErrorTypes
        public async Task<ActionResult> Index()
        {
            return View(await db.ErrorType.ToListAsync());
        }

    

        // GET: ErrorTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ErrorTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ErrorTypeID,Description")] ErrorType errorType)
        {
            if (ModelState.IsValid)
            {
                db.ErrorType.Add(errorType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(errorType);
        }

        // GET: ErrorTypes/Edit/5
        public async Task<ActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ErrorType errorType = await db.ErrorType.FindAsync(id);
            if (errorType == null)
            {
                return HttpNotFound();
            }
            return View(errorType);
        }

        // POST: ErrorTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ErrorTypeID,Description")] ErrorType errorType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(errorType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(errorType);
        }

        // GET: ErrorTypes/Delete/5
        public async Task<ActionResult> Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ErrorType errorType = await db.ErrorType.FindAsync(id);
            if (errorType == null)
            {
                return HttpNotFound();
            }
            return View(errorType);
        }

        // POST: ErrorTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(byte id)
        {
            ErrorType errorType = await db.ErrorType.FindAsync(id);
            db.ErrorType.Remove(errorType);
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
