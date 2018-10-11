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
using CFAStudentTracker.Models.ViewModels;
using System.Collections.Generic;

namespace CFAStudentTracker.Controllers
{
    public class SystemManagementController : Controller
    {
        private CFAEntities db = new CFAEntities();
        // GET: SystemManagement
        public ActionResult Index()
        {
            SystemManagementVM vm = new SystemManagementVM();
            return View(vm);
        }
        public ActionResult NoSSNErrorResolve(long id)
        {
            ProcessingError error = db.ProcessingError.Find(id);
            db.ProcessingError.Remove(error);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DupSSNResolve(string ssn)
        {
            var studentFiles = db.StudentFile.Include(p=>p.Record).Where(p => p.FileSSN == ssn);
            List<List<ProcessingVM>> filesProcessed = new List<List<ProcessingVM>>();
            foreach (var item in studentFiles)
            {
                List<ProcessingVM> listVM = new List<ProcessingVM>();
                foreach (var r in item.Record)
                {
                    
                    foreach (var p in r.Processing)
                    {
                        ProcessingVM v = new ProcessingVM();
                        if (p.Record.ProcPriority)
                        {
                            v.status = "Priority";
                            if (p.ProcToUser == null)
                            {
                                v.status += " - In Queue";
                            }
                            else if (p.InFilingCabinet)
                            {
                                v.status += " - In Cabinet";
                            }
                            else if (p.ProcUserComplete != null)
                            {
                                v.status += " - Completed";
                            }
                            else
                            {
                                v.status += " - In User Processing";
                            }
                        }
                        else if (p.ProcToUser == null)
                        {
                            v.status = "In Queue";
                        }
                        else if (p.InFilingCabinet)
                        {
                            v.status = "In Cabinet";
                        }
                        else if (p.ProcUserComplete != null)
                        {
                            v.status += "Completed";
                        }
                        else
                        {
                            v.status = "In User Processing";
                        }
                        v.p = p;
                        if (p.Record.DOD != null)
                        {
                            v.ProcDate = DateTime.Parse(p.Record.DOD.ToString()).ToString("MM/dd/yyyy");
                        }
                        listVM.Add(v);
                    }
                    
                }
                filesProcessed.Add(listVM);
            }
            return View(filesProcessed);
        }
        public ActionResult ResolveSSN(long id)
        {
            var CorrectStudentFile = db.StudentFile.Find(id);
            var IncorrectStudentFiles = db.StudentFile.Where(p => p.FileSSN == CorrectStudentFile.FileSSN && p.FileID != CorrectStudentFile.FileID);
            foreach (var item in IncorrectStudentFiles)
            {
                var r = db.Record.Where(p => p.FileID == item.FileID);
                foreach (var ri in r)
                {
                    ri.FileID = CorrectStudentFile.FileID;
                    db.Entry(ri).State = EntityState.Modified;
                }
                db.StudentFile.Remove(item);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}