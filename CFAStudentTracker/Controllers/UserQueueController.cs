using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using CFAStudentTracker.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Data.Entity;


namespace CFAStudentTracker.Controllers
{
    [Authorize]
    public class UserQueueController : Controller
    {               
        private CFAEntities db = new CFAEntities();
        public async Task<ActionResult> Index()
        {
            var user = User.Identity.Name;
            
            var iUser = db.User.Include(u => u.Queue).Where(u => u.Username == user).First();
            var queues = iUser.Queue;
            foreach (var q in queues)
            {
                var max = q.FilesInQueue;
                var queID = q.QueueID;
                var pr = db.Processing.Include(p => p.Record).Where(p => p.ProcUserComplete == null && p.Username == user && p.InFilingCabinet == false && p.QueueID == queID);
                for (int i = 0; i < max; i++)
                {
                    if (pr.ToList().Count < max)
                    {
                        Helpers helper = new Helpers();
                        await helper.GetNextFile(queID, User.Identity.Name);
                    }
                }
                
            }

            var proc = db.Processing.Include(p => p.Record).Include(p=>p.Queue).Where(p => p.ProcUserComplete == null && p.Username == user && p.InFilingCabinet == false);

            List<UserQueue_Result> records = new List<UserQueue_Result>();
            foreach (var item in proc)
            {
                UserQueue_Result i = new UserQueue_Result();
                i.ID = item.ProcID;
                i.Name = item.Record.StudentFile.FileName;
                i.SSN = item.Record.StudentFile.FileSSN;
                i.Type = item.Record.FileType.TypeDescription;
                i.IsPriority = item.Record.ProcPriority;
                i.DateIn = item.ProcToUser;
                i.QueueName = item.Queue.QueueDescription;
                records.Add(i);
            }
            Helpers h = new Helpers();

            double proctoday = h.WeightedFileAmount(DateTime.Today, DateTime.Today.AddDays(1), user);
            ViewBag.CompletedToday = proctoday;
                return View(records.OrderByDescending(r => r.IsPriority).ThenBy(r=>r.DateIn));
           
        }

     }
}
