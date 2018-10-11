using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CFAStudentTracker.Controllers
{
    public class ProcessingListController : Controller
    {
        // GET: ProcessingList
        public ActionResult Index(string json)
        {
            List<Processing> procList = JsonConvert.DeserializeObject<List<Processing>>(json);
            
            return View(procList.ToList());
        }
    }
}