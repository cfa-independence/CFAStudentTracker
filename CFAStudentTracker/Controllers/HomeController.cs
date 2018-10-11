using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using System.Data.OleDb;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using CFAStudentTracker.Models.ViewModels;

namespace CFAStudentTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        CFAEntities db = new CFAEntities();
        [Authorize(Roles = "Admin,Officer,QC Officer")]
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            Session["datatable"] = dt;
            ViewBag.UserQueue = db.GetQueues(User.Identity.Name).Count();
            ViewBag.FilingCabinet = db.UserFilingCabinet(User.Identity.Name).Count();
            var queues = db.GetQueues(User.Identity.Name).ToList();
            DashboardViewModel dvm = new DashboardViewModel(User.Identity.Name);
            foreach (var item in queues)
            {
                dvm.queueStats.Add(new QueueStat(db.Queue.Find(item.ID)));

            }
            return View(dvm);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Search(string SSN)
        {
            
            if (SSN == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SSN = SSN.Trim(new Char[] { ' ', '-'});
            SSN = SSN.Replace("-", "");
            SSN = SSN.Insert(5, "-").Insert(3, "-");
            List<FileDetail> dt = new List<FileDetail>();
            var files = db.StudentFile.Include(s=>s.Record).Where(s => s.FileSSN.Contains(SSN));
            var q = from p in db.Processing
                        join r in db.Record
                            on p.RecordID equals r.RecordID
                        join t in db.FileType
                            on r.FileTypeID equals t.FileTypeID
                        join f in db.StudentFile
                            on r.FileID equals f.FileID
                        where f.FileSSN.Contains(SSN)
                        orderby p.ProcID
                        select new LookUp { p=p, r=r, f=f, t=t};
            
            if (files.ToList().Count == 0)
            {
                return HttpNotFound();
            }
            ViewBag.mainReturn = Url.Action("Search", "Home", new { SSN = SSN });
            return View(q);
        }
        [AllowAnonymous]
        public ActionResult FileDetail(int id)
        {
            ViewBag.FileName = db.StudentFile.Find(id).FileName;
            var records = db.Record.Where(r => r.FileID == id);
            return View(records);
        }
        [AllowAnonymous]
        public ActionResult RecordDetail(long id, string name)
        {
            var record = db.Record.Include(r => r.Processing).Include(r => r.Note).Include(r => r.StudentFile).Include(r => r.FileType).Where(r => r.RecordID == id).First();
            return View(record);
        }

    }
}