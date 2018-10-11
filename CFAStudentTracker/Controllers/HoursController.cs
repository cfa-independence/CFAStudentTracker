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
    public class HoursController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: Hours
        public async Task<ActionResult> Index(string user)
        {
            var hour = db.Hour.Include(h => h.User).Where(h=>h.Username == user).OrderByDescending(h => h.HourDate);
            return View(await hour.ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AdminIndex(string user)
        {
            var hour = db.Hour.Include(h => h.User).Where(h => h.Username == user).OrderByDescending(h=>h.HourDate);
            ViewBag.Username = user;
            return View(await hour.ToListAsync());
        }

        // GET: Hours/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create(string user)
        {
            ViewBag.Username = user;
            return View();
        }

        // POST: Hours/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "HourID,HourDate,HourAmount,HourNotes,Username")] Hour hour)
        {
            var i = db.Hour.Where(h => h.HourDate == hour.HourDate && h.Username == hour.Username);
            if (ModelState.IsValid && i.ToList().Count == 0)
            {
                db.Hour.Add(hour);
                await db.SaveChangesAsync();
                return RedirectToAction("AdminIndex", new {user = hour.Username });
            }

            ViewBag.Username = hour.Username;
            return View(hour);
        }

        // GET: Hours/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hour hour = await db.Hour.FindAsync(id);
            if (hour == null)
            {
                return HttpNotFound();
            }
            ViewBag.Username = hour.Username;
            return View(hour);
        }

        // POST: Hours/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "HourID,HourDate,HourAmount,HourNotes,Username")] Hour hour)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hour).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("AdminIndex", new { user = hour.Username });
            }
            ViewBag.Username = hour.Username;
            return View(hour);
        }

        // GET: Hours/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hour hour = await db.Hour.FindAsync(id);
            if (hour == null)
            {
                return HttpNotFound();
            }
            return View(hour);
        }

        // POST: Hours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Hour hour = await db.Hour.FindAsync(id);
            db.Hour.Remove(hour);
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
