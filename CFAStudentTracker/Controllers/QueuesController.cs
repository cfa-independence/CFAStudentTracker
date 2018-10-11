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
using CFAStudentTracker.Models.ViewModels;

namespace CFAStudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QueuesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: Queues
        public ActionResult Index(short? id)
        {
            
            DataTable secondT = new DataTable();
            if (Session["datatable"] != null)
            {
                secondT = ((DataTable)Session["datatable"]);
            }
            var queue = db.Queue.Include(q => q.Group).Include(q => q.Queue2).Include(q => q.QueueOrder).Where(q => q.GroupID == id);
            List<QueueVM> vm = new List<QueueVM>();
            foreach (var item in queue)
            {
                QueueVM v = new QueueVM();
                if((db.Queue.Where(p => p.QueueID == item.QueueNextQueue)).Select(q => q.QueueDescription).Count() > 0)
                {
                    v.NextQueue = (db.Queue.Where(p => p.QueueID == item.QueueNextQueue)).Select(q => q.QueueDescription).First();
                }
                if (db.Processing.Where(p => p.ProcUserComplete == null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.ProcInQueue).Count() > 0)
                {
                    v.OldestFileInQueue = Math.Round((double)(DateTime.Today - (db.Processing.Where(p => p.ProcUserComplete == null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.ProcInQueue).First().ProcInQueue)).TotalDays,2);
                }
                var dodList = db.Processing.Include(path=>path.Record).Where(p => p.ProcUserComplete == null && p.Record.DOD != null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.Record.DOD);
                if (dodList.Count() > 0)
                {
                    v.OldestProcDate = DateTime.Parse(dodList.OrderBy(p => p.Record.DOD).First().Record.DOD.ToString()).ToString("MM/dd/yyyy");
                }

                var fq = db.FilesInQueue(item.QueueID).Sum(p => p.fileAmount).ToString();
                var uq = db.UsersInQueue(item.QueueID).Count().ToString();
                if (fq == "")
                    v.FilesInQueue = 0;
                else
                    v.FilesInQueue = Int32.Parse(fq);
                if (uq == "")
                    v.UsersInQueue = 0;
                else
                    v.UsersInQueue = Int32.Parse(uq);
                var avg = (db.Processing.Where(p => p.ProcUserComplete >= DbFunctions.AddDays(DateTime.Now, -60) && p.Username != null && p.QueueID == item.QueueID).Average(p => DbFunctions.DiffDays(p.ProcInQueue, p.ProcUserComplete)));
                if (avg == null)
                {
                    v.AvgQueueTime = 0;
                } else
                {
                    v.AvgQueueTime = Math.Round((double)avg, 2);
                }
                v.Auditing = item.AuditQueue;
                v.q = item;
                vm.Add(v);
            }
            
            ViewBag.GroupName = queue.First().Group.GroupName.ToString() ;
            return View(vm);
        }

        // GET: Queues/Details/5
        public ActionResult Details(short? id)
        {
            DataTable secondT = new DataTable();
            if (Session["datatable"] != null)
            {
                secondT = ((DataTable)Session["datatable"]);
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Queue queue = db.Queue.Include(path=>path.Group).Include(p=>p.QueueOrder).Where(p=>p.QueueID == id).First();
            if (queue == null)
            {
                return HttpNotFound();
            }
            QueueViewModel qvm = new QueueViewModel();
            qvm.queueStats = new QueueStat(queue);
            qvm.queue = db.Queue.Include(path => path.Group).Include(p => p.QueueOrder).Where(p => p.QueueID == id).First(); ;
            qvm.Setup();
            var assignedAudit = db.Queue.Find(queue.AuditQueueAssigned);
            if (assignedAudit != null)
            {
                qvm.SendAuditQueue = assignedAudit.QueueDescription;
                qvm.precentAudit = queue.AuditPercent;
            } else
            {
                qvm.SendAuditQueue = "";
                qvm.precentAudit = 0;
            }
            
            ViewBag.NextQueue = "";
            if (qvm.queue.Queue2 != null)
            {
                ViewBag.NextQueue = qvm.queue.Queue2.QueueDescription;
            }
            return View(qvm);
        }
        
            public ActionResult UserQueue(string userName)
        {
            
            var userQueue = db.Processing.Include(p=>p.Record).Include(p => p.Record.StudentFile)
                .Where(p=>p.Username == userName && p.ProcUserComplete == null ).ToList();
            ProcessingList pl = new ProcessingList();
            var vm = pl.Setup(userQueue);
            return View("../ProcessingList/Index", vm);
        }
        // GET: Queues/Create
        public ActionResult Create()
        {
            ViewBag.GroupID = new SelectList(db.Group, "GroupID", "GroupName");
            ViewBag.QueueNextQueue = new SelectList(db.Queue, "QueueID", "QueueDescription");
            ViewBag.QueueOrderID = new SelectList(db.QueueOrder, "QueueOrderID", "QueueOrderDescription");
            return View();
        }

        // POST: Queues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "QueueID,QueueDescription,QueueNextQueue,GroupID,QueueOrderID")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                db.Queue.Add(queue);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = queue.GroupID });
            }

            ViewBag.GroupID = new SelectList(db.Group, "GroupID", "GroupName", queue.GroupID);
            ViewBag.QueueNextQueue = new SelectList(db.Queue, "QueueID", "QueueDescription", queue.QueueNextQueue);
            ViewBag.QueueOrderID = new SelectList(db.QueueOrder, "QueueOrderID", "QueueOrderDescription", queue.QueueOrderID);
            return View(queue);
        }

        // GET: Queues/Edit/5
        public async Task<ActionResult> Edit(short? id)
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
            ViewBag.GroupID = new SelectList(db.Group, "GroupID", "GroupName", queue.GroupID);
            ViewBag.QueueNextQueue = new SelectList(db.Queue, "QueueID", "QueueDescription", queue.QueueNextQueue);
            ViewBag.QueueOrderID = new SelectList(db.QueueOrder, "QueueOrderID", "QueueOrderDescription", queue.QueueOrderID);
            return View(queue);
        }

        // POST: Queues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QueueID,QueueDescription,QueueNextQueue,GroupID,QueueOrderID,FilesInQueue,AuditQueue,AuditQueueAssigned,AuditPercent")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(queue).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new {id = queue.GroupID });
            }
            ViewBag.GroupID = new SelectList(db.Group, "GroupID", "GroupName", queue.GroupID);
            ViewBag.QueueNextQueue = new SelectList(db.Queue, "QueueID", "QueueDescription", queue.QueueNextQueue);
            ViewBag.QueueOrderID = new SelectList(db.QueueOrder, "QueueOrderID", "QueueOrderDescription", queue.QueueOrderID);
            return View(queue);
        }
        // GET: Queues/Edit/5
        public async Task<ActionResult> Audit(short? id)
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
            ViewBag.AuditQueueAssigned = new SelectList(db.Queue, "QueueID", "QueueDescription", queue.QueueNextQueue);
            return View(queue);
        }

        // POST: Queues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Audit([Bind(Include = "QueueID,QueueDescription,QueueNextQueue,GroupID,QueueOrderID,FilesInQueue,AuditQueue,AuditQueueAssigned,AuditPercent")] Queue queue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(queue).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = queue.GroupID });
            }
            ViewBag.AuditQueueAssigned = new SelectList(db.Queue, "QueueID", "QueueDescription", queue.QueueNextQueue);
            return View(queue);
        }


        // GET: Queues/Delete/5
        public async Task<ActionResult> Delete(short? id)
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
            return View(queue);
        }

        // POST: Queues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(short id)
        {
            Queue queue = await db.Queue.FindAsync(id);
            db.Queue.Remove(queue);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = queue.GroupID });
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
