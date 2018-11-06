using CFAStudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CFAStudentTracker.Controllers
{
    [Authorize]
    public class TimeCategoryController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: TimeCategory
        public async Task<ActionResult> Index()
        {
            return View(await db.TimeCategory.ToListAsync());
        }

        // GET: TimeCategory/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TimeCategory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TimeCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TimeCategoryId, TimeCategoryTitle")] TimeCategory timeCategory)
        {
            if (ModelState.IsValid)
            {
                db.TimeCategory.Add(timeCategory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(timeCategory);
        }

        // GET: TimeCategory/Edit/5
        public async Task<ActionResult> Edit(short? id)
        {
            if (id != null)
            {
                TimeCategory timeCategory = await db.TimeCategory.FindAsync(id);
                if (timeCategory != null)
                {
                    return View(timeCategory);
                }
            }

            return HttpNotFound();
        }

        // POST: TimeCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TimeCategoryId, TimeCategoryTitle")] TimeCategory timeCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(timeCategory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(timeCategory);
        }

        // GET: TimeCategory/Delete/5
        public async  Task<ActionResult> Delete(short? id)
        {
            if (id != null)
            {
                TimeCategory timeCategory = await db.TimeCategory.FindAsync(id);
                if (timeCategory != null)
                {
                    return View(timeCategory);
                }
            }
            
            return HttpNotFound();            
        }

        // POST: TimeCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(short id)
        {
            TimeCategory timeCategory = await db.TimeCategory.FindAsync(id);
            db.TimeCategory.Remove(timeCategory);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetTimeCategories()
        {
            List<TimeCategory> timeCategories = db.TimeCategory.ToList();
            List<dynamic> dto = new List<dynamic>();

            foreach (TimeCategory timeCategory in timeCategories)
            {
                dto.Add(new
                {
                    categoryId = timeCategory.TimeCategoryId,
                    title = timeCategory.TimeCategoryTitle
                });
            }

            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTimeCategory(int id)
        {
            TimeCategory timeCategory = db.TimeCategory.Where(c => c.TimeCategoryId == id).FirstOrDefault();
            var category = new
            {
                categoryId = timeCategory.TimeCategoryId,
                title = timeCategory.TimeCategoryTitle
            };

            return Json(category, JsonRequestBehavior.AllowGet);
        }
    }
}
