using CFAStudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CFAStudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        CFAEntities db = new CFAEntities();
        // GET: UserSchedules
        public ActionResult Index()
        {
            var i = db.User;
            return View(i);
        }
        
        
    }
}