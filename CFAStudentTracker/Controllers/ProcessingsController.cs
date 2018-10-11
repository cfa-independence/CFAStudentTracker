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
    [Authorize(Roles = "Admin")]
    public class ProcessingsController : Controller
    {
        private CFAEntities db = new CFAEntities();

        // GET: Processings
        public ActionResult Index(short? i, string userName)
        {
            if (i == null)
            {
                return RedirectToAction("../Queue/Index");
            }
            ViewBag.queueID = i;
            var processing = db.Processing.Include(p => p.Queue).Include(p => p.Record).Include(p => p.User).Include(p => p.Record.StudentFile).Where(p => p.QueueID == i).Where(p => p.ProcUserComplete == null);
            if (userName != null)
            {
                processing.Where(p => p.Username == userName);
            }

            List<ProcessingVM> vm = new List<ProcessingVM>();
            foreach (var item in processing)
            {
                ProcessingVM v = new ProcessingVM();
                if (item.Record.ProcPriority)
                {
                    v.status = "Priority";
                    if (item.ProcToUser == null)
                    {
                        v.status += " - In Queue";
                    }
                    else if (item.InFilingCabinet)
                    {
                        v.status += " - In Cabinet";
                    }
                    else
                    {
                        v.status += " - In User Processing";
                    }
                } else if (item.ProcToUser == null)
                {
                    v.status = "In Queue";
                } else if (item.InFilingCabinet)
                {
                    v.status = "In Cabinet";
                }
                else
                {
                    v.status = "In User Processing";
                }
                v.p = item;
                if(item.Record.DOD != null)
                {
                    v.ProcDate = DateTime.Parse(item.Record.DOD.ToString()).ToString("MM/dd/yyyy");
                }
                
                vm.Add(v);
            }
            var order = db.Queue.Find(i);
            if (order.QueueOrderID == 3)
            {
                return View(vm.OrderBy(p => p.p.Record.DOD));
            }
            return View(vm.OrderBy(p=>p.p.ProcID));
        }

        public ActionResult ChangePriority(string rID, string rP, string queue)
        {
            Record rec = db.Record.Find(long.Parse(rID));
            short x = Int16.Parse(queue);
            if (rP == "False")
            {
                rec.ProcPriority = true;
            } else
            {
                rec.ProcPriority = false;
            }


            db.Entry(rec).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new {i=x });
        }

        // GET: Processings/Create
        public ActionResult Create()
        {
            ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription");
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription");
            return View();
        }

        // POST: Processings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "QueueID")] Processing processing, 
                                               [Bind(Include = "ProcPriority, DOD, LDA, FileTypeID")] Record record,
                                               [Bind(Include = "FileSSN, FileName")] StudentFile sFile,
                                               string note)
        {
            if (ModelState.IsValid)
            {
                Note n = new Note();
                n.Note1 = note;
                n.Username = User.Identity.Name;
                if (db.StudentFile.Where(p => p.FileSSN == sFile.FileSSN).Count() > 0)
                {
                    sFile = db.StudentFile.Where(p => p.FileSSN == sFile.FileSSN).First();
                }
                else
                {
                    db.StudentFile.Add(sFile);
                    db.SaveChanges();
                }
                processing.ProcInQueue = DateTime.Now;
                record.StudentFile = sFile;
                record.Processing.Add(processing);
                record.Note.Add(n);
                db.Record.Add(record);
                db.SaveChanges();

                //db.InsertIntoQueue(sFile.FileSSN,
                //                   sFile.FileName,
                //                   record.FileTypeID,
                //                   processing.QueueID,
                //                   note,
                //                   "",
                //                   User.Identity.Name,
                //                   record.DOD,
                //                   record.LDA);
                //if (record.ProcPriority)
                //{
                //    var rec = db.Processing.Where(p => p.Record.StudentFile.FileSSN == sFile.FileSSN).OrderByDescending(p => p.ProcInQueue).First().RecordID;
                //    var re = db.Record.Find(rec);
                //    re.ProcPriority = true;
                //    db.Entry(re).State = EntityState.Modified;
                //    db.SaveChanges();
                //}


                return RedirectToAction("Index", new { i = processing.QueueID });
            } else
            {
                ViewBag.QueueID = new SelectList(db.Queue, "QueueID", "QueueDescription");
                ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription");
                record.Processing.Add(processing);
                record.StudentFile = sFile;
                return View(record);
            }
            
        }

        public ActionResult Batch(short? id)
        {
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription");
            ViewBag.QueueID = id;
            return View();
        }

        [HttpPost]
        public ActionResult Batch(HttpPostedFileBase file, string FileTypeID, string QueueID)
        {            
            DataSet StudentFile = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {

                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    //------------------------------------------------------
                    //SELECT column names below
                    //------------------------------------------------------
                    string query = string.Format("Select SSN, Student, Note1, Note2, ProcessDate  from [{0}]", excelSheets[0]);
                    //------------------------------------------------------
                    //SELECT column names above
                    //------------------------------------------------------
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(StudentFile);
                    }
                    
                }

                short typeID = short.Parse(FileTypeID);
                short queueID = short.Parse(QueueID);
                DataRowCollection rows = StudentFile.Tables[0].Rows;
                foreach (DataRow row in rows)
                {
                    DateTime? ProcessDate = null;
                    string procDateString = row[4].ToString();
                    if (!String.IsNullOrEmpty(procDateString.ToString()))
                    {
                        ProcessDate = DateTime.Parse(procDateString);
                    }

                    string ssn = row[0].ToString();
                    if (!ssn.Contains("-"))
                    {
                        ssn = ssn.Substring(0, 3) + "-" + ssn.Substring(3, 2) + "-" + ssn.Substring(5);
                    }
                    string studentName = row[1].ToString();
                    string note1 = row[2].ToString();
                    string note2 = row[3].ToString();

                    if (!String.IsNullOrEmpty(ssn) && !String.IsNullOrEmpty(studentName))
                        //Insert column numbers into DB
                        db.InsertIntoQueue(ssn,
                            studentName,
                            typeID,
                            queueID,
                            note1,
                            note2,
                            User.Identity.Name,
                            ProcessDate,
                            null);
                }                               
            }

            return RedirectToAction("Index", new {i = QueueID });
        }

        public ActionResult BatchRemove(short? id)
        {
            ViewBag.QueueID = id;
            return View();
        }

        [HttpPost]
        public ActionResult BatchRemove(HttpPostedFileBase file, string QueueID)
        {

            DataSet StudentFile = new DataSet();
            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension =
                                     System.IO.Path.GetExtension(Request.Files["file"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation))
                    {

                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                    //------------------------------------------------------
                    //SELECT column names below
                    //------------------------------------------------------
                    string query = string.Format("Select ProcID, Note  from [{0}]", excelSheets[0]);
                    //------------------------------------------------------
                    //SELECT column names above
                    //------------------------------------------------------
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(StudentFile);
                    }

                }
                

                short queueID = short.Parse(QueueID);
                for (int i = 0; i < StudentFile.Tables[0].Rows.Count; i++)
                {
                    long procID = long.Parse(StudentFile.Tables[0].Rows[i][0].ToString());

                    var proc = db.Processing.Find(procID);
                    Note note = new Note();
                    note.Username = User.Identity.Name;
                    note.RecordID = proc.RecordID;
                    note.Note1 = StudentFile.Tables[0].Rows[i][1].ToString();
                    if (ModelState.IsValid)
                    {
                        db.Note.Add(note);
                        db.SaveChanges();
                    }

                    proc.InFilingCabinet = false;
                    proc.ProcToUser = DateTime.Now;
                    proc.ProcUserComplete = DateTime.Now;
                    proc.Username = null;
                    if (ModelState.IsValid)
                    {
                        db.Entry(proc).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index", new { i = QueueID });
        }
        // GET: Processings/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Processing processing = await db.Processing.FindAsync(id);
            if (processing == null)
            {
                return HttpNotFound();
            }
            return View(processing);
        }

        // POST: Processings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Processing processing = await db.Processing.FindAsync(id);
            db.Processing.Remove(processing);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { i = processing.QueueID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // GET: UserQueue/Edit/5
        public ActionResult Open(long id)
        {
            ProcessingDetail re = new ProcessingDetail();
            re.Proc = db.Processing.Include(p => p.Queue).Where(p => p.ProcID == id).ToList()[0];
            re.Rec = db.Record.Include(r => r.Processing).Include(r => r.Note).Include(r => r.StudentFile).Include(r => r.FileType).Where(r => r.RecordID == re.Proc.RecordID).ToList()[0];
            return View(re);
        }

        // GET: Queues/Create
        public ActionResult AddNote(string ProcessingID)
        {
            ViewBag.ProcessingID = ProcessingID;
            long x = long.Parse(ProcessingID);
            var i = db.Processing.Where(p => p.ProcID == x).ToList()[0];
            Note note = new Note();
            note.RecordID = i.RecordID;
            note.Username = i.Username;
            return View(note);
        }

        // POST: Queues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNote([Bind(Include = "Note1,RecordID,Username")] Note note, string ProcessingID)
        {
            note.Username = User.Identity.Name;
            if (ModelState.IsValid)
            {
                db.Note.Add(note);
                await db.SaveChangesAsync();
                return RedirectToAction("Open", new { id = ProcessingID });
            }

            return View(note);
        }
        public ActionResult Reassign(string ProcessingID)
        {
            ViewBag.Username = new SelectList(db.User, "Username", "Username");
            long i = long.Parse(ProcessingID);
            return View(db.Processing.Find(i));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reassign([Bind(Include = "ProcID,InFilingCabinet,ProcInQueue,ProcToUser,ProcUserComplete,Username,QueueID,RecordID")] Processing processing)
        {
            if (processing.ProcToUser == null)
            {
                processing.ProcToUser = DateTime.Now;
            }
            if (processing.Username == null)
            {
                processing.ProcToUser = null;
            }
            if (ModelState.IsValid)
            {
                db.Entry(processing).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { i=processing.QueueID});
            }

            ViewBag.Username = new SelectList(db.User, "Username", "Username");
            return View(db.Processing.Find(processing.ProcID));
        }

        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Record record = await db.Record.FindAsync(id);
            if (record == null)
            {
                return HttpNotFound();
            }
            ViewBag.PreviousPage = Request.UrlReferrer.AbsolutePath;
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", record.FileTypeID);
            return View(record);
        }

        // POST: Processings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RecordID, ProcPriority,DOD,LDA,FileTypeID,FileID,FileSSN,FileName")] Record record, [Bind(Include = "FileID,FileSSN,FileName")] StudentFile studentFile, string PreviousPage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(record).State = EntityState.Modified;
                db.Entry(studentFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction(".." + PreviousPage);
            }
            //
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", record.FileTypeID);
            return View(record);
        }
    }
}
