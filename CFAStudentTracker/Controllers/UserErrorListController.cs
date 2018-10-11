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
    public class UserErrorListController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: UserErrorList
        public async Task<ActionResult> Index(string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            var processing = db.ProcessingError.Include(p => p.Processing).Include(p => p.ErrorType).Include(p => p.ErrorComplete).Where(p=>p.ErrorComID == null && p.Processing.Username == username).Select(p=>p.Processing).Distinct();
            return View(await processing.ToListAsync());
        }

        public ActionResult CompletedErrors(string username)
        {
            IndexFilter u = new IndexFilter();
            u.userName = username;
            return View(u);
        }
        [HttpPost]
        public ActionResult CompletedErrors([Bind(Include = "startDate,endDate,userName")] IndexFilter filter)
        {
            DateTime dtEnd = (DateTime)filter.endDate;
            DateTime dtStart = (DateTime)filter.startDate;
            var errorFiles = db.ProcessingError.Include(p => p.Processing).Include(p => p.Processing.Queue).Include(p => p.ErrorType).Include(p => p.ErrorType).Where(p => p.DateComplete <= dtEnd && p.DateComplete >= dtStart && p.Processing.Username == filter.userName).Select(p => p.Processing).Distinct();
            return View("Index", errorFiles.ToList());
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
