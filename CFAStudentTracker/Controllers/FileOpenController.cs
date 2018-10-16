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
    public class FileOpenController : Controller
    {
        private CFAEntities db = new CFAEntities();
        private MembershipEntities dbUser = new MembershipEntities();

        private Dictionary<int, decimal> SubAmounts = new Dictionary<int, decimal>()
            {
                { 0, 0 },
                { 1, 3500 },
                { 2, 4500 },
                { 3, 5500 },
                { 6, 0 }
            };

        private Dictionary<int, decimal> IndependentUnsubAmounts = new Dictionary<int, decimal>()
            {
                { 0, 0 },
                { 1, 6000 },
                { 2, 6000 },
                { 3, 7000 },
                { 6, 20500 }
            };

        private Dictionary<string, double> SulaTermMultipliers = new Dictionary<string, double>()
            {
                { "full-time", 1 },
                { "three-quarter-time", 0.75 },
                { "half-time", 0.5 },
                { "less-than-half-time", 0.25 }
            };

        private ProcessingDetail GetProcessingDetail(long id)
        {
            ViewBag.id = id;
            ProcessingDetail re = new ProcessingDetail();
            re.Proc = db.Processing.Include(p => p.Queue).Where(p => p.ProcID == id).ToList()[0];
            re.Rec = db.Record.Include(r => r.Processing).Include(r => r.Note).Include(r => r.StudentFile).Include(r => r.FileType).Where(r => r.RecordID == re.Proc.RecordID).ToList()[0];

            List<FileDetail> dt = new List<FileDetail>();
            var files = db.StudentFile.Include(s => s.Record).Where(s => s.FileID == re.Rec.FileID);
            var q = from p in db.Processing
                    join r in db.Record
                        on p.RecordID equals r.RecordID
                    join t in db.FileType
                        on r.FileTypeID equals t.FileTypeID
                    join f in db.StudentFile
                        on r.FileID equals f.FileID
                    where f.FileID == re.Rec.FileID
                    orderby p.ProcID
                    select new LookUp { p = p, r = r, f = f, t = t };
            re.PreviousProcessed = q;
            re.ProcErrors = db.ProcessingError.Include(p => p.ErrorType).Include(p => p.ErrorComplete).Where(p => p.ProcID == id);

            return re;
        }

        public ActionResult Calculate(ProcessingDetail detail, string mainReturn)
        {
            Record record = db.Record.Find(detail.Rec.RecordID);
            record.DependencyStatus = detail.Rec.DependencyStatus;
            record.AcademicYear = detail.Rec.AcademicYear;
            record.SubAggLimit = detail.Rec.SubAggLimit;
            record.CombinedAggLimit = detail.Rec.CombinedAggLimit;
            record.IsProratedLoan = detail.Rec.IsProratedLoan;
            record.NumCredits = detail.Rec.NumCredits;
            record.ExistingAYEndsBeforeTermTwo = detail.Rec.ExistingAYEndsBeforeTermTwo;
            record.SubAmountUsed = detail.Rec.SubAmountUsed;
            record.UnsubAmountUsed = detail.Rec.UnsubAmountUsed;
            record.SumUsagePeriods = detail.Rec.SumUsagePeriods;
            record.AttendanceTermOne = detail.Rec.AttendanceTermOne;
            record.AttendanceTermTwo = detail.Rec.AttendanceTermTwo;
            record.NumAcademicYearsInProgram = detail.Rec.NumAcademicYearsInProgram;
            record.AwardYear = detail.Rec.AwardYear;
            record.EFC = detail.Rec.EFC;
            record.LEU = detail.Rec.LEU;
            record.PercentPellUsed = detail.Rec.PercentPellUsed;
            record.StatusTermOne = detail.Rec.StatusTermOne;
            record.StatusTermTwo = detail.Rec.StatusTermTwo;
            record.StatusTermThree = detail.Rec.StatusTermThree;
            record.IsOnlineStudent = detail.Rec.IsOnlineStudent;
            record.BudgetAwardYear = detail.Rec.BudgetAwardYear;
            record.StateOnISIR = detail.Rec.StateOnISIR;
            record.IsWithParents = detail.Rec.IsWithParents;
            record.NumEstimatedCredits = detail.Rec.NumEstimatedCredits;
            record.CostPerCredit = detail.Rec.CostPerCredit;
            record.NumMonthsInAY = detail.Rec.NumMonthsInAY;

            db.Entry(record).State = EntityState.Modified;
            db.SaveChanges();            

            return Redirect(mainReturn);
        }

        public ActionResult OpenFile(long id, string mainReturn)
        {            
            if (String.IsNullOrEmpty(mainReturn))
            {
                ViewBag.mainReturn = Request.UrlReferrer;

            }
            else
            {
                ViewBag.mainReturn = mainReturn;
            }
            var userN = dbUser.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
            if (userN.AspNetRoles.First().Name == "QC Officer" || userN.AspNetRoles.First().Name == "Admin")
            {
                return Redirect(Url.Action("OpenAdmin", "FileOpen", new { id, mainReturn = ViewBag.mainReturn }));
            }
            ViewBag.OpenReturn = Url.Action("OpenFile", "FileOpen", new { id, mainReturn = ViewBag.mainReturn });
            ProcessingDetail re = GetProcessingDetail(id);

            return View(re);
        }
        [Authorize(Roles = "Admin,QC Officer")]
        public ActionResult OpenAdmin(long id, string mainReturn)
        {
            if (String.IsNullOrEmpty(mainReturn))
            {
                ViewBag.mainReturn = Request.UrlReferrer;

            }
            else
            {
                ViewBag.mainReturn = mainReturn;
            }
            ViewBag.OpenReturn = Url.Action("OpenAdmin", "FileOpen", new { id, mainReturn = ViewBag.mainReturn });
            ProcessingDetail re = GetProcessingDetail(id);

            CalculateViewBags(re);
            return View(re);
        }
        #region ProcessingFile

        private void CalculateViewBags(ProcessingDetail processingDetail)
        {
            Record record = processingDetail.Rec;

            // Set Defaults (perhaps a DB table for this?)
            decimal StartingSub;
            decimal StartingUnsub = 2000;
            decimal AvailableAggSub = 23000;
            decimal AvailableAggCombine = 31000;
            int AcademicYear = Convert.ToInt32(record.AcademicYear);

            StartingSub = record.DependencyStatus == "DependentNoParentInfo" ? 0 : SubAmounts[AcademicYear];
            if (record.DependencyStatus == "Independent" || record.DependencyStatus == "DependentOverride")
            {
                StartingUnsub = IndependentUnsubAmounts[AcademicYear];
                AvailableAggCombine = 57500;
            }
            else if (AcademicYear == 6)
            {
                StartingUnsub = 20500;
                AvailableAggCombine = 138500;
            }

            decimal SulaSub = 0;
            decimal SulaUnsub = StartingSub + StartingUnsub;

            switch (SulaCalc(record, StartingSub, StartingUnsub))
            {
                case "ALL":
                    SulaSub = StartingSub;
                    SulaUnsub = StartingUnsub;
                    break;
                case "HALF":
                    SulaSub = StartingSub / 2;
                    SulaUnsub = StartingUnsub / 2;
                    break;
                default:
                    break;
            }

            decimal ProratedSub = StartingSub;
            decimal ProratedUnsub = StartingUnsub;
            decimal ProrateSulaSub = ProratedSub;
            decimal ProrateSulaUnsub = ProratedUnsub;

            if (record.IsProratedLoan)
            {
                decimal NumCredits = Convert.ToDecimal(record.NumCredits);
                ProratedSub = NumCredits / 36 * StartingSub;
                ProratedUnsub = NumCredits / 36 * StartingUnsub;

                ProrateSulaSub = (SulaSub < ProratedSub) ? SulaSub : ProratedSub;
                ProrateSulaUnsub = (SulaUnsub < ProratedUnsub) ? SulaUnsub : ProratedUnsub;
            }

            decimal RemainingAggSub = 23000;
            decimal RemainingAggUnsub = 31000;            

            if (AcademicYear == 6)
            {
                RemainingAggSub = 65500;
            }
            else if (record.DependencyStatus != null && (record.DependencyStatus == "Independent" || record.DependencyStatus == "DependentOverride"))
            {
                RemainingAggUnsub = 57500;
            }
            RemainingAggSub = RemainingAggSub - Convert.ToDecimal(record.SubAmountUsed);
            RemainingAggUnsub = RemainingAggUnsub - Convert.ToDecimal(record.UnsubAmountUsed);

            decimal AggReallocSub;
            decimal AggReallocUnsub;
            decimal AggReallocSubCombine;
            decimal AggReallocUnsubCombine;

            AggReallocUnsub = AggReallocSub = RemainingAggUnsub < 0 ? 0 : Math.Max(ProrateSulaSub, RemainingAggSub);
            AggReallocSubCombine = AggReallocSub == RemainingAggSub ? ProrateSulaUnsub + (ProrateSulaSub - RemainingAggSub) : ProrateSulaUnsub;
            AggReallocUnsubCombine = Math.Max(AggReallocSubCombine, RemainingAggUnsub - AggReallocUnsub);

            decimal ExistingLoanRemaining = AggReallocUnsub - Convert.ToDecimal(record.SubAmountUsed);
            decimal ExistingLoanRemainingCombine = AggReallocUnsubCombine - Convert.ToDecimal(record.UnsubAmountUsed);

            decimal ExistingReallocSub = Math.Min(ExistingLoanRemaining, AggReallocUnsub);
            decimal ExistingReallocSubCombine = Math.Min(ExistingLoanRemainingCombine, AggReallocUnsubCombine);
            decimal ExistingReallocUnsub = ExistingReallocSub;
            decimal ExistingReallocUnsubCombine;

            if (AggReallocUnsub - ExistingReallocSub > 0 && ExistingLoanRemainingCombine - ExistingReallocSubCombine > 0)
            {
                if (AggReallocUnsub - ExistingReallocSub + ExistingReallocSubCombine > ExistingLoanRemainingCombine)
                {
                    ExistingReallocUnsubCombine = ExistingLoanRemainingCombine;
                }
                else
                {
                    ExistingReallocUnsubCombine = AggReallocUnsub - ExistingReallocSub + ExistingReallocSubCombine;
                }
            }
            else
            {
                ExistingReallocUnsubCombine = ExistingReallocSubCombine;
            }

            decimal FinalSub = Math.Max(ExistingReallocUnsub, 0);
            decimal FinalUnsub = Math.Max(ExistingReallocUnsubCombine, 0);

            ViewBag.FinalMaxSub = String.Format("{0:C2}", FinalSub);
            ViewBag.FinalMaxUnsub = String.Format("{0:C2}", FinalUnsub);

            if (record.SubAmountUsed != null)
            {
                ViewBag.MaxRemainingSub = String.Format("{0:C2}", Math.Floor(StartingSub - (decimal)record.SubAmountUsed));
            }

            if (record.UnsubAmountUsed != null)
            {
            ViewBag.MaxRemainingUnsub   = String.Format("{0:C2}", Math.Floor(StartingUnsub - (decimal)record.UnsubAmountUsed));                
            }            

            if (record.SubAggLimit != null)
            {
                ViewBag.AvailableAggSub = String.Format("{0:C2}", Math.Floor(AvailableAggSub - (decimal)record.SubAggLimit));
            }

            if (record.CombinedAggLimit != null)
            {
                ViewBag.AvailableAggCombine = String.Format("{0:C2}", Math.Floor(AvailableAggCombine - (decimal)record.CombinedAggLimit));
            }

            if (record.ExistingAYEndsBeforeTermTwo != null)
            {
                ViewBag.LoanPeriod = (bool)record.ExistingAYEndsBeforeTermTwo ? "8 Month" : "4 Month";
            }

            if (record.NumAcademicYearsInProgram != null)
            {
                ViewBag.UsagePeriod = record.NumAcademicYearsInProgram * 1.5;
            }

            if (record.SumUsagePeriods != null)
            {
                ViewBag.RemainingUsagePeriod = ViewBag.UsagePeriod - record.SumUsagePeriods;                
            }

            ViewBag.MaxProrateSub       = String.Format("{0:C2}", Math.Floor(ProratedSub));
            ViewBag.MaxProrateUnsub     = String.Format("{0:C2}", Math.Floor(ProratedUnsub));
            ViewBag.StartingSub         = String.Format("{0:C2}", Math.Floor(StartingSub));
            ViewBag.StartingUnsub       = String.Format("{0:C2}", Math.Floor(StartingUnsub));

        }

        public string SulaCalc(Record record, decimal StartingSub, decimal StartingUnsub)
        {
            string SulaResult = String.Empty;

            if (StartingSub == 0)
                return "ALL";

            double Term1Usage = record.AttendanceTermOne != null ? SulaTermMultipliers[record.AttendanceTermOne] / 2 : 0;
            double Term2Usage = record.AttendanceTermTwo != null ? SulaTermMultipliers[record.AttendanceTermTwo] / 2 : 0;
            double RemainingUsage;

            if (record.SumUsagePeriods == null)
            {
                RemainingUsage = 9999;
            }
            else
            {
                RemainingUsage = Convert.ToDouble(record.NumAcademicYearsInProgram) * 1.5 - Convert.ToDouble(record.SumUsagePeriods);
            }

            if (Term1Usage + Term2Usage <= RemainingUsage)
            {
                return "ALL";
            }
            else if (Term2Usage <= Term1Usage)
            {
                return "HALF";
            }
            else
            {
                return "NONE";
            }            
        }

        public async Task<ActionResult> CompleteFile(string pID, string qID, string mainReturn)
        {
            var queueID = short.Parse(qID);
            var procID = long.Parse(pID);
            var x = db.Processing.Find(procID);
            var username = x.Username;
            var i = db.User.Where(u => u.Username == username).Include(p => p.Queue).Where(p => p.Queue.Where(q => q.QueueID == queueID).Count() > 0).Count();
            var student = db.Processing.Include(p => p.Queue).Where(p => p.ProcID == procID).First();
            if (student.Queue.QueueNextQueue != null)
            {
                if (db.Processing.Where(p => p.RecordID == student.RecordID && p.QueueID == student.Queue.QueueNextQueue).Count() == 0)
                {
                    Processing newProcessing = new Processing();
                    newProcessing.InFilingCabinet = false;
                    newProcessing.ProcessingError = null;
                    newProcessing.ProcInQueue = DateTime.Now;
                    newProcessing.ProcToUser = null;
                    newProcessing.ProcUserComplete = null;
                    newProcessing.QueueID = student.Queue.QueueNextQueue.Value;
                    newProcessing.RecordID = student.RecordID;
                    newProcessing.Username = null;
                    db.Processing.Add(newProcessing);
                    await db.SaveChangesAsync();
                    var nextStudent = db.Processing.Find(newProcessing.ProcID);
                    if (nextStudent.ProcInQueue == null)
                    {
                        return Redirect(mainReturn); ;
                    }
                }

            }
            x.ProcUserComplete = DateTime.Now;
            db.Entry(x).State = EntityState.Modified;
            await db.SaveChangesAsync();
            if (i > 0)
            {


                var user = User.Identity.Name;

                var iUser = db.User.Include(u => u.Queue).Where(u => u.Username == user).First();
                var queues = iUser.Queue;
                foreach (var q in queues)
                {
                    var qu = q.FilesInQueue;
                    var countInQueue = db.Processing.Where(t => t.Username == username && t.ProcUserComplete == null && t.InFilingCabinet == false && t.QueueID == q.QueueID).Count();
                    while (qu > countInQueue)
                    {
                        Helpers h = new Helpers();
                        long e = await h.GetNextFile(q.QueueID, User.Identity.Name);
                        if (e < 0)
                        {
                            break;
                        }
                        countInQueue = db.Processing.Where(t => t.Username == username && t.ProcUserComplete == null && t.InFilingCabinet == false && t.QueueID == q.QueueID).Count();
                    }

                }


            }

            //audit selection
            var AuditQueue = db.Queue.Find(queueID);
            if (AuditQueue.AuditQueue)
            {
                var count = db.Processing.Where(p => p.QueueID == AuditQueue.AuditQueueAssigned && p.ProcUserComplete == null).Count();

                if (count < AuditQueue.AuditPercent)
                {
                    Processing auditProc = new Processing();
                    db.InsertIntoQueue(student.Record.StudentFile.FileSSN,
                            student.Record.StudentFile.FileName,
                            22,
                            AuditQueue.AuditQueueAssigned,
                            null,
                            null,
                            null,
                            null,
                            null);
                }
            }
            //Sure up QUEUE. Make sure there is not an error.
            var filesTo = db.Processing.Where(p => p.Username == null && p.ProcUserComplete == null && p.ProcToUser != null);
            foreach (var item in filesTo)
            {
                item.ProcToUser = null;
                item.Username = null;
                item.InFilingCabinet = false;
                db.Entry(item).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();

            return Redirect(mainReturn);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult UncompleteFile(string pID, string mainReturn)
        {
            Processing p = db.Processing.Find(long.Parse(pID));
            p.ProcUserComplete = null;
            db.Entry(p).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect(mainReturn);
        }

        public async Task<ActionResult> FilingCabinet(string pID, string mainReturn)
        {
            long x = long.Parse(pID);
            var i = db.Processing.Where(p => p.ProcID == x).ToList()[0];
            if (i.InFilingCabinet)
            {
                i.InFilingCabinet = false;
            }
            else
            {
                i.InFilingCabinet = true;

            }
            if (ModelState.IsValid)
            {
                db.Entry(i).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }

        // GET: Queues/Create
        public ActionResult AddNote(string ProcessingID, string mainReturn)
        {
            ViewBag.ProcessingID = ProcessingID;
            ViewBag.mainReturn = mainReturn;
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
        public async Task<ActionResult> AddNote([Bind(Include = "Note1,RecordID,Username")] Note note, string ProcessingID, string mainReturn)
        {
            note.Username = User.Identity.Name;
            ViewBag.mainReturn = mainReturn;
            if (ModelState.IsValid)
            {
                db.Note.Add(note);
                await db.SaveChangesAsync();
                return Redirect(mainReturn);
            }

            return View(note);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult ChangePriority(string rID, string rP, string queue, string mainReturn)
        {
            Record rec = db.Record.Find(long.Parse(rID));
            short x = Int16.Parse(queue);
            if (rP == "False")
            {
                rec.ProcPriority = true;
            }
            else
            {
                rec.ProcPriority = false;
            }


            db.Entry(rec).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect(mainReturn);
        }
        public ActionResult Reassign(string ProcessingID, string mainReturn)
        {
            var userN = dbUser.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
            long i = long.Parse(ProcessingID);
            Processing processing = db.Processing.Find(i);
            if (processing.ProcUserComplete == null || userN.AspNetRoles.First().Name == "Admin")
            {
                ViewBag.mainReturn = mainReturn;
                ViewBag.Username = new SelectList(db.User, "Username", "Username");
                return View(processing);
            }
            return View(db.Processing.Find(processing.ProcID));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reassign([Bind(Include = "ProcID,InFilingCabinet,ProcInQueue,ProcToUser,ProcUserComplete,Username,QueueID,RecordID")] Processing processing, string mainReturn)
        {

            string noteString = "";
            ViewBag.mainReturn = mainReturn;
            if (processing.ProcToUser == null)
            {
                processing.ProcToUser = DateTime.Now;
            }
            if (processing.Username == null)
            {
                noteString = "queue";
                processing.ProcToUser = null;
            }
            else
            {
                noteString = processing.Username;
            }
            Note note = new Note();
            note.Username = User.Identity.Name;
            note.RecordID = processing.RecordID;
            note.Note1 = "Reassigned to " + noteString;
            if (ModelState.IsValid)
            {
                db.Note.Add(note);
                await db.SaveChangesAsync();
            }
            if (ModelState.IsValid)
            {
                db.Entry(processing).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Redirect(mainReturn);
            }

            ViewBag.Username = new SelectList(db.User, "Username", "Username");
            return View(db.Processing.Find(processing.ProcID));
        }

        public async Task<ActionResult> Edit(long? id, string mainReturn)
        {
            ViewBag.mainReturn = mainReturn;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Record record = await db.Record.FindAsync(id);
            if (record == null)
            {
                return HttpNotFound();
            }
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", record.FileTypeID);
            return View(record);
        }

        // POST: Processings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RecordID, ProcPriority,DOD,LDA,FileTypeID,FileID,FileSSN,FileName")] Record record, [Bind(Include = "FileID,FileSSN,FileName")] StudentFile studentFile, string mainReturn)
        {
            ViewBag.mainReturn = mainReturn;
            if (ModelState.IsValid)
            {
                db.Entry(record).State = EntityState.Modified;
                db.Entry(studentFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Redirect(mainReturn);
            }
            //
            ViewBag.FileTypeID = new SelectList(db.FileType, "FileTypeID", "TypeDescription", record.FileTypeID);
            return View(record);
        }

        // GET: ErrorManagement/Create
        public ActionResult Create(long? id, string mainReturn)
        {
            ViewBag.mainReturn = mainReturn;
            ProcessingError i = new ProcessingError();
            i.ProcID = id;
            ViewBag.ErrorTypeID = new SelectList(db.ErrorType, "ErrorTypeID", "Description");
            return View(i);
        }

        // POST: ErrorManagement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ErrorID,ProcID,ErrorTypeID,ErrorComID,DateFound,DateComplete,Note")] ProcessingError processingError, string mainReturn)
        {
            if (ModelState.IsValid)
            {
                processingError.DateFound = DateTime.Now;
                db.ProcessingError.Add(processingError);
                await db.SaveChangesAsync();
                return Redirect(mainReturn);
            }
            ViewBag.ErrorTypeID = new SelectList(db.ErrorType, "ErrorTypeID", "Description", processingError.ErrorTypeID);
            return View(processingError);
        }
        #endregion

        #region Errors
        [Authorize(Roles = "Admin")]
        public ActionResult EditError(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            ViewBag.mainReturn = mainReturn;
            ViewBag.ErrorTypeID = new SelectList(db.ErrorType, "ErrorTypeID", "Description", processingError.ErrorType);
            return View(processingError);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult EditError([Bind(Include = "ErrorID,ProcID,ErrorTypeID,ErrorComID,DateFound,DateComplete,Note")] ProcessingError processingError, string mainReturn)
        {
            if (ModelState.IsValid)
            {
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }
        public ActionResult CompleteError(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            ViewBag.mainReturn = mainReturn;
            ViewBag.ErrorComID = new SelectList(db.ErrorComplete, "ErrorComID", "Description", processingError.ErrorComID);
            return View(processingError);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteError([Bind(Include = "ErrorID,ProcID,ErrorTypeID,ErrorComID,DateFound,DateComplete,Note")] ProcessingError processingError, string mainReturn)
        {
            if (ModelState.IsValid)
            {
                processingError.DateComplete = DateTime.Now;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult UncompleteError(string error, string mainReturn)
        {
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            if (ModelState.IsValid)
            {
                processingError.DateComplete = null;
                processingError.ErrorComID = null;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }
        public ActionResult ReassignError(string error, string mainReturn)
        {
            var userN = dbUser.AspNetUsers.Where(r => r.UserName == User.Identity.Name).First();
            var p = long.Parse(error);
            var processingError = db.ProcessingError.Find(p);
            if (processingError.DateComplete == null || userN.AspNetRoles.First().Name == "Admin")
            {
                var fid = processingError.Processing.Record.FileID;
                ViewBag.mainReturn = mainReturn;
                ViewBag.error = error;
                var list = db.Processing.Include(c => c.Record).Where(c => c.Record.FileID == fid);
                return View(list);
            }

            return Redirect(mainReturn);
        }
        public ActionResult ReassignFinal(string error, string procID, string mainReturn)
        {
            var err = long.Parse(error);
            var pro = long.Parse(procID);
            var processingError = db.ProcessingError.Find(err);
            if (ModelState.IsValid)
            {
                processingError.ProcID = pro;
                db.Entry(processingError).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(mainReturn);
            }
            return Redirect(mainReturn);
        }

        public ActionResult Route(string mainReturn)
        {
            return Redirect(mainReturn);

        }
        [Authorize(Roles = "Admin")]
        // GET: ErrorCompletes/Delete/5
        public async Task<ActionResult> DeleteError(long id, string mainReturn)
        {
            ViewBag.mainReturn = mainReturn;
            ProcessingError errorComplete = await db.ProcessingError.FindAsync(id);
            if (errorComplete == null)
            {
                return HttpNotFound();
            }
            return View(errorComplete);
        }

        // POST: ErrorCompletes/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteError")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id, string mainReturn)
        {
            ProcessingError errorComplete = await db.ProcessingError.FindAsync(id);
            var returnID = errorComplete.ProcID;
            db.ProcessingError.Remove(errorComplete);
            await db.SaveChangesAsync();
            return RedirectToAction("OpenAdmin", new { id = returnID, mainReturn = mainReturn });
        }
        #endregion


    }

}