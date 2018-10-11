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
    [Authorize]
    public class UserFilingCabinetController : Controller
    {
        private CFAEntities db = new CFAEntities();
        public ActionResult Index()
        {
            var records = db.UserFilingCabinet(User.Identity.Name).OrderByDescending(p=>p.ID);

            return View(records);
        }

      
    }
}