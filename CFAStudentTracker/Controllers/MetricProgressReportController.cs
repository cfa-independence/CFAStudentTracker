using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data.Entity;

namespace CFAStudentTracker.Controllers
{
    public class MetricProgressReportController : Controller
    {
        // GET: MetricProgressReport
        private CFAEntities db = new CFAEntities();
        public ActionResult Index(string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }
            
            
            MetricProgressViewModel mp = new MetricProgressViewModel(username);
            
            
            
            
            
            return View(mp);
        }
        public ActionResult ErrorList(string username, DateTime lastUpdated, int days)
        {
            DateTime begin = lastUpdated.AddDays(days*-1);
            DateTime end = lastUpdated.AddDays(1);
            var list = db.ProcessingError.Include(p => p.ErrorComplete).Include(p => p.ErrorType).Include(p => p.Processing).Where(p => p.Processing.Username == username && p.DateFound >= begin && p.DateFound <= end);
            return View(list);
        }
    }
}