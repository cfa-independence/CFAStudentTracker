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
    public class TimeEntryController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: TimeEntry
        public ActionResult Index()
        {            
            List<TimeEntry> timeEntries = db.TimeEntry
                .Where(e => e.TimeEntryEnd != null)
                .Where(e => e.User.SupervisorName == User.Identity.Name)
                .Where(e => !e.IsApproved)
                .OrderBy(e => e.Username).ThenBy(e => e.TimeEntryStart)
                .ToList();

            return View(timeEntries);
        }

        // GET: TimeEntry/Details/5
        public ActionResult Details(string username, DateTime? date)
        {
            return View(new DashboardViewModel(username, date, User.Identity.Name));
        }

        // GET: TimeEntry/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TimeEntry/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: TimeEntry/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TimeEntry/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: TimeEntry/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TimeEntry/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> StartTimer(DateTime start, string description, int categoryId)
        {
            User user = db.User.Find(User.Identity.Name);
            TimeEntry timeEntry = new TimeEntry
            {
                TimeCategoryId = categoryId,
                TimeEntryStart = start,
                TimeEntryDescription = description,
                Username = user.Username
            };

            db.TimeEntry.Add(timeEntry);
            await db.SaveChangesAsync();

            return Json(new { entryId = timeEntry.TimeEntryId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> StopTimer(int? entryId, DateTime end)
        {
            if (entryId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TimeEntry timeEntry = db.TimeEntry.Find(entryId);
            if (timeEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            timeEntry.TimeEntryEnd = end;

            db.Entry(timeEntry).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult GetTimerInProgress()
        {
            TimeEntry timeEntry = db.TimeEntry.Where(e => e.TimeEntryEnd == null && e.Username == User.Identity.Name).OrderByDescending(e => e.TimeEntryStart).FirstOrDefault();
            if (timeEntry == null)
            {
                return Content("");
            }
            var entry = new
            {
                entryId = timeEntry.TimeEntryId,
                categoryId = timeEntry.TimeCategoryId,
                description = timeEntry.TimeEntryDescription,
                startTime = timeEntry.TimeEntryStart,
                endTime = timeEntry.TimeEntryEnd
            };

            return Json(entry, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTimeEntriesForDate(string username, DateTime date)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            DashboardViewModel dvm = new DashboardViewModel(username, date, User.Identity.Name);

            return PartialView("_TimeEntryTable", dvm);
        }


        [HttpPost]
        public ActionResult DeleteTimeEntry(int id, DateTime date, string username)
        {
            TimeEntry timeEntry = db.TimeEntry.Find(id);
            if (timeEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            db.TimeEntry.Remove(timeEntry);
            db.SaveChanges();
            return RedirectToAction("GetTimeEntriesForDate", new { username, date });
        }

        [HttpPost]
        public ActionResult UpdateTimeEntry(int id, string username, string description, int categoryId, DateTime startDateTime, DateTime? endDateTime)
        {
            TimeEntry timeEntry = db.TimeEntry.Find(id);
            if (timeEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            timeEntry.TimeCategoryId = categoryId;
            timeEntry.TimeEntryDescription = description;
            timeEntry.TimeEntryStart = startDateTime;
            timeEntry.IsApproved = false;
            if (endDateTime.HasValue)
            {
                timeEntry.TimeEntryEnd = endDateTime.Value;
            }

            db.Entry(timeEntry).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("GetTimeEntriesForDate", new { username, date = startDateTime });
        }

        [HttpPost]
        public ActionResult SetTimeEntryApproval(int id, string username, bool shouldApprove, DateTime startDateTime)
        {
            TimeEntry timeEntry = db.TimeEntry.Find(id);
            if (timeEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            timeEntry.IsApproved = shouldApprove;
            db.Entry(timeEntry).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("GetTimeEntriesForDate", new { username, date = startDateTime });
        }

        [HttpPost]
        public async Task<ActionResult> ApproveTimeEntry(int id)
        {
            TimeEntry timeEntry = await db.TimeEntry.FindAsync(id);
            if (timeEntry != null)
            {
                timeEntry.IsApproved = true;
                db.Entry(timeEntry).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetUnapprovedCount()
        {
            int count =  db.TimeEntry
                .Where(e => e.TimeEntryEnd != null)
                .Where(e => e.User.SupervisorName == User.Identity.Name)
                .Where(e => !e.IsApproved)
                .Count();

            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }
    }
}
