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
    public class WeightedFilesController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: WeightedFiles
        public async Task<ActionResult> Index(short? id)
        {
            var weightedFile = db.WeightedFile.Include(w => w.FileType).Include(w => w.Queue).Where(p=>p.QueueID == id);
            ViewBag.QueueID = id;
            return View(await weightedFile.ToListAsync());
        }

        // GET: WeightedFiles/Create
        public ActionResult Create(short? id)
        {
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription");
            ViewBag.QueueID = id;
            return View();
        }

        // POST: WeightedFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "QueueID,FileTypeID,WeightedAmount")] WeightedFile weightedFile)
        {
            if (ModelState.IsValid)
            {
                db.WeightedFile.Add(weightedFile);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new {id = weightedFile.QueueID });
            }

            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", weightedFile.FileTypeID);
            ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription", weightedFile.QueueID);
            return View(weightedFile);
        }

        // GET: WeightedFiles/Edit/5
        public ActionResult Edit(short? id, short? fid)
        {
            
            WeightedFile weightedFile = db.WeightedFile.Where(p=>p.QueueID == id && p.FileTypeID == fid).First();
            ViewBag.FileTypeName = weightedFile.FileType.TypeDescription;
            return View(weightedFile);
        }

        // POST: WeightedFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QueueID,FileTypeID,WeightedAmount")] WeightedFile weightedFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(weightedFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new {id = weightedFile.QueueID });
            }
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", weightedFile.FileTypeID);
            ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription", weightedFile.QueueID);
            return View(weightedFile);
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
