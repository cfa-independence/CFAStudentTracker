using CFAStudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CFAStudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MasterMetricProgressReportController : Controller
    {
        private CFAEntities db = new CFAEntities();
        // GET: MasterMetricProgressReport
        public ActionResult Index(short id)
        {
            MasterMPRVM i = new MasterMPRVM(id);
            return View(i);
        }
    }
}