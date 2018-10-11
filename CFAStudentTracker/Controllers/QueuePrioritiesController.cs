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
    public class QueuePrioritiesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: QueuePriorities
        public async Task<ActionResult> Index(short? QueueID, byte? index, bool? up)
        {
            var qo = new QOVM();
            qo.QueueID = QueueID;
            qo.QOList = db.QueuePriority.Include(q => q.FileType).Include(q => q.Queue).Where(q => q.QueueID == QueueID).OrderBy(q => q.QPOrder);
            if (index != null)
            {
                await Reorder(qo.QOList, (byte)index, (bool)up);
                qo.QOList = db.QueuePriority.Include(q => q.FileType).Include(q => q.Queue).Where(q => q.QueueID == QueueID).OrderBy(q => q.QPOrder);
            }
            return View(qo);
        }

        private async Task Reorder(IEnumerable<QueuePriority> Order, byte index, bool up)
        {
            var mx = Order.Max(o => o.QPOrder);
            if (up && (index - 1) >= 0)
            {
                byte next = (byte)(index - 1);
                var find = Order.Where(q => q.QPOrder == next).ToList()[0];
                var current = Order.Where(q => q.QPOrder == index).ToList()[0];
                db.Entry(find).State = EntityState.Deleted;
                db.Entry(current).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                find.QPOrder = index;
                current.QPOrder = next;
                db.Entry(find).State = EntityState.Added;
                db.Entry(current).State = EntityState.Added;
                await db.SaveChangesAsync();
            } else if (!up && index < mx)
            {
                byte next = (byte)(index + 1);
                var find = Order.Where(q => q.QPOrder == next).ToList()[0];
                var current = Order.Where(q => q.QPOrder == index).ToList()[0];
                db.Entry(find).State = EntityState.Deleted;
                db.Entry(current).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                find.QPOrder = index;
                current.QPOrder = next;
                db.Entry(find).State = EntityState.Added;
                db.Entry(current).State = EntityState.Added;
                await db.SaveChangesAsync();
            }

        }

        public ActionResult QueueOrderChange(short? id)
        {
            var i = db.Queue.Find(id);
            ViewBag.QueueOrderID = new SelectList(db.QueueOrder, "QueueOrderID", "QueueOrderDescription", i.QueueOrderID);
            ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription", id);
            return View();
        }
        [HttpPost]
        public ActionResult QueueOrderChange(string QueueID, string QueueOrderID)
        {
            Queue queue = db.Queue.Find(short.Parse(QueueID));

            queue.QueueOrderID = short.Parse(QueueOrderID);


            db.Entry(queue).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { QueueID = QueueID });
        }


       

        // GET: QueuePriorities/Create
        public ActionResult Create(string QueueID)
        {
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription");
            ViewBag.QueueID = QueueID;
            return View();
        }

        // POST: QueuePriorities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FileTypeID")] QueuePriority queuePriority, string QueueID)
        {
            queuePriority.QueueID = short.Parse(QueueID);
            if (db.QueuePriority.Where(q=>q.QueueID == queuePriority.QueueID && q.FileTypeID == queuePriority.FileTypeID).ToList().Count == 1)
            {
                return RedirectToAction("Index", new { QueueID = QueueID});
            }
            queuePriority.QPOrder = (byte)db.QueuePriority.Where(q => q.QueueID == queuePriority.QueueID).ToList().Count;
            
            if (ModelState.IsValid)
            {
                db.QueuePriority.Add(queuePriority);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { QueueID = QueueID });
            }

            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", queuePriority.FileTypeID);
            ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription", queuePriority.QueueID);
            return View(queuePriority);
        }
        // GET: QueuePriorities/Delete/5
        public ActionResult Delete(string QPOrder, string QueueID)
        {
            byte bQPOrder = byte.Parse(QPOrder);
            short bQueueID = short.Parse(QueueID);
            QueuePriority queuePriority = db.QueuePriority.Where(q=>q.QPOrder == bQPOrder && q.QueueID==bQueueID ).First();
            if (queuePriority == null)
            {
                return HttpNotFound();
            }
            return View(queuePriority);
        }

        // POST: QueuePriorities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed([Bind(Include = "QueueID,FileTypeID,QPOrder")] QueuePriority queuePriority)
        {
            var Q = db.Queue.Find(queuePriority.QueueID);
            List<QueuePriority> newList = new List<QueuePriority>();
            byte i = 0;
            foreach (var item in Q.QueuePriority)
            {
                if (item.QPOrder != queuePriority.QPOrder)
                {
                    item.QPOrder = i;
                    newList.Add(item);
                    i++;
                }
            }
            Q.QueuePriority = newList;
            db.Entry(Q).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index", Q.QueueID);
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
