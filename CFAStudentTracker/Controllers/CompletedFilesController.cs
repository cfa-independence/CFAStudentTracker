using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace CFAStudentTracker.Controllers
{
    [Authorize]
    public class CompletedFilesController : Controller
    {
        // GET: CompletedFiles
        public ActionResult Index(string username)
        {
            IndexFilter u = new IndexFilter();
            if (String.IsNullOrEmpty(username))
            {
                username = User.Identity.GetUserName();
            }
            u.userName = username;
            return View(u);
        }
        [HttpPost]
        public ActionResult Index([Bind(Include = "startDate,endDate,userName")] IndexFilter filter)
        {
            IEnumerable<Processing> comFiles;
                comFiles = filter.GetCompleteFiles();
            
            return View("Results", comFiles);
        }
    }
}