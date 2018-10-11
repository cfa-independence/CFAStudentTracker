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
    public class ErrorOpenController : Controller
    {
        private CFAEntities db = new CFAEntities();
        private MembershipEntities dbUser = new MembershipEntities();
        // GET: ErrorOpen
        public ActionResult OpenAdmin(long e, string mainReturn)
        {
            if (String.IsNullOrEmpty(mainReturn))
            {
                ViewBag.mainReturn = Request.UrlReferrer;

            }
            else
            {
                ViewBag.mainReturn = mainReturn;
            }
            
            ViewBag.OpenReturn = Url.Action("Open", "ErrorOpen", new { e = e, mainReturn = ViewBag.mainReturn });
            var vm = new ErrorOpenVM(e);
            return View(vm);
        }
        public ActionResult Open(long e, string mainReturn)
        {
            if (String.IsNullOrEmpty(mainReturn))
            {
                ViewBag.mainReturn = Request.UrlReferrer;

            }
            else
            {
                ViewBag.mainReturn = mainReturn;
            }
            var userN = dbUser.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
            if (userN.AspNetRoles.First().Name == "Admin")
            {
                return Redirect(Url.Action("OpenAdmin", "ErrorOpen", new { e = e, mainReturn = ViewBag.mainReturn }));
            }
            ViewBag.OpenReturn = Url.Action("Open", "ErrorOpen", new { e = e, mainReturn = ViewBag.mainReturn });
            var vm = new ErrorOpenVM(e);
            return View(vm);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult EditError(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            ViewBag.mainReturn = mainReturn;
            ViewBag.ErrorTypeID = new SelectList(db.ErrorType, "ErrorTypeID", "Description", processingError.ErrorType);
            return View(processingError);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult EditError([Bind(Include = "ErrorID,ProcID,ErrorTypeID,ErrorComID,DateFound,DateComplete,Note")] ProcessingError processingError, string mainReturn)
        {
            if (ModelState.IsValid)
            {
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }
        public ActionResult CompleteFile(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            ViewBag.mainReturn = mainReturn;
            ViewBag.ErrorComID = new SelectList(db.ErrorComplete, "ErrorComID", "Description", processingError.ErrorComID);
            return View(processingError);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteFile([Bind(Include = "ErrorID,ProcID,ErrorTypeID,ErrorComID,DateFound,DateComplete,Note")] ProcessingError processingError, string mainReturn)
        {
            if (ModelState.IsValid)
            {
                processingError.DateComplete = DateTime.Now;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }
        public ActionResult Uncomplete(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            if (ModelState.IsValid)
            {
                processingError.DateComplete = null;
                processingError.ErrorComID = null;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }

            return Redirect(mainReturn);
        }
        public ActionResult Reassign(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            var fid = processingError.Processing.Record.FileID;
            ViewBag.mainReturn = mainReturn;
            ViewBag.error = error;
            var list = db.Processing.Include(c => c.Record).Where(c=>c.Record.FileID == fid);
            return View(list);
        }
        public ActionResult ReassignFinal(string error, string procID, string mainReturn)
        {
            var err = long.Parse(error);
            var pro = long.Parse(procID);
            var processingError = db.ProcessingError.Find(err);
            if (ModelState.IsValid)
            {
                processingError.ProcID = pro;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }

        public ActionResult Route(string mainReturn)
        {
            return Redirect(mainReturn);
            
        }
        [Authorize(Roles = "Admin")]
        // GET: ErrorCompletes/Delete/5
        public async Task<ActionResult> DeleteError(long id)
        {
            ProcessingError errorComplete = await db.ProcessingError.FindAsync(id);
            if (errorComplete == null)
            {
                return HttpNotFound();
            }
            return View(errorComplete);
        }

        // POST: ErrorCompletes/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteError")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            ProcessingError errorComplete = await db.ProcessingError.FindAsync(id);
            var returnID = errorComplete.ProcID;
            db.ProcessingError.Remove(errorComplete);
            await db.SaveChangesAsync();
            return RedirectToAction("../FileOpen/OpenAdmin", new { id = returnID });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UncompleteError(long id)
        {
            var processingError = db.ProcessingError.Find(id);
            if (ModelState.IsValid)
            {
                processingError.DateComplete = null;
                processingError.ErrorComID = null;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("../FileOpen/OpenAdmin", new { id = processingError.ProcID });
            }
            return RedirectToAction("../FileOpen/OpenAdmin", new { id = processingError.ProcID });
        }
    }

}