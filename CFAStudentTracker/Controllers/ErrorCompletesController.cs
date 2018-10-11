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
    public class ErrorCompletesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: ErrorCompletes
        public async Task<ActionResult> Index()
        {
            return View(await db.ErrorComplete.ToListAsync());
        }


        // GET: ErrorCompletes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ErrorCompletes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Description,Counted")] ErrorComplete errorComplete)
        {
            if (ModelState.IsValid)
            {
                db.ErrorComplete.Add(errorComplete);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(errorComplete);
        }

        // GET: ErrorCompletes/Edit/5
        public async Task<ActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ErrorComplete errorComplete = await db.ErrorComplete.FindAsync(id);
            if (errorComplete == null)
            {
                return HttpNotFound();
            }
            return View(errorComplete);
        }

        // POST: ErrorCompletes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ErrorComID,Description,Counted")] ErrorComplete errorComplete)
        {
            if (ModelState.IsValid)
            {
                db.Entry(errorComplete).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(errorComplete);
        }

        // GET: ErrorCompletes/Delete/5
        public async Task<ActionResult> Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ErrorComplete errorComplete = await db.ErrorComplete.FindAsync(id);
            if (errorComplete == null)
            {
                return HttpNotFound();
            }
            return View(errorComplete);
        }

        // POST: ErrorCompletes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(byte id)
        {
            ErrorComplete errorComplete = await db.ErrorComplete.FindAsync(id);
            db.ErrorComplete.Remove(errorComplete);
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
