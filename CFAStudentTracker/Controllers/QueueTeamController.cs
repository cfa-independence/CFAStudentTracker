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
    [Authorize(Roles = "Admin")]
    public class QueueTeamController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: QueueTeam
        public ActionResult Index(short? id)
        {
            var queue = db.Queue.Find(id);
            return View(queue);
        }
        // GET: QueueTeam/Create
        public ActionResult Create(short? id)
        {
            ViewBag.Username = new SelectList(db.User, "Username", "Username");
            ViewBag.id = id;
            return View();
        }

        // POST: QueueTeam/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create(short id, string Username)
        {
            var queue = db.Queue.Find(id);
            var user = db.User.Find(Username);
            if (user != null && ModelState.IsValid)
            {
                queue.User.Add(user);
                db.Entry(queue).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = id});
            }

            ViewBag.Username = new SelectList(db.User, "Username", "Username");
            ViewBag.QueueID = id;
            return View();
        }

        // GET: QueueTeam/Delete/5
        public async Task<ActionResult> Delete(short? id, string user)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = await db.Queue.FindAsync(id);
            if (queue == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = id;
            ViewBag.user = user;
            return View();
        }

        // POST: QueueTeam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(short id, string user)
        {
            Queue queue = await db.Queue.FindAsync(id);
            var user1 = queue.User.Where(q => q.Username == user).First();
            var filesTo = db.Processing.Where(p => p.Username == user && p.ProcUserComplete == null & p.InFilingCabinet == false);
            foreach (var item in filesTo)
            {
                item.ProcToUser = null;
                item.Username = null;
                item.InFilingCabinet = false;
                db.Entry(item).State = EntityState.Modified;
            }
                queue.User.Remove(user1);
                db.Entry(queue).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = id });
            
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
