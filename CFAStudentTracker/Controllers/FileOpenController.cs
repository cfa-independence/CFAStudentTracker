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

        private readonly string DecimalAmountFormat = "{0:C2}";
        private readonly string DecimalPercentFormat = "{0:#.0000}%";

        private struct CLE
        {
            public CLE(decimal r, decimal t, decimal p)
            {
                Room = r;
                Travel = t;
                Personal = p;
            }

            public decimal Room;
            public decimal Travel;
            public decimal Personal;
        };

        private struct PellAmounts
        {
            public PellAmounts(decimal ft, decimal tqt, decimal ht, decimal bht)
            {
                FullTime = ft;
                ThreeQuarterTime = tqt;
                HalfTime = ht;
                BelowHalfTime = bht;
            }

            public decimal FullTime;
            public decimal ThreeQuarterTime;
            public decimal HalfTime;
            public decimal BelowHalfTime;
        };

        private Dictionary<Func<decimal, bool>, PellAmounts> PellByEFC = new Dictionary<Func<decimal, bool>, PellAmounts>()
        {
            { value => value == 0, new PellAmounts(6095, 4571, 3048, 1524) },
            { value => value >= 1 && value <= 100, new PellAmounts(6045, 4534, 3023, 1511) },
            { value => value >= 101 && value <= 200, new PellAmounts(5945, 4459, 2973, 1486) },
            { value => value >= 201 && value <= 300, new PellAmounts(5845, 4384, 2923, 1461) },
            { value => value >= 301 && value <= 400, new PellAmounts(5745, 4309, 2873, 1436) },
            { value => value >= 401 && value <= 500, new PellAmounts(5645, 4234, 2823, 1411) },
            { value => value >= 501 && value <= 600, new PellAmounts(5545, 4159, 2773, 1386) },
            { value => value >= 601 && value <= 700, new PellAmounts(5445, 4084, 2723, 1361) },
            { value => value >= 701 && value <= 800, new PellAmounts(5345, 4009, 2673, 1336) },
            { value => value >= 801 && value <= 900, new PellAmounts(5245, 3934, 2623, 1311) },
            { value => value >= 901 && value <= 1000, new PellAmounts(5145, 3859, 2573, 1286) },
            { value => value >= 1001 && value <= 1100, new PellAmounts(5045, 3784, 2523, 1261) },
            { value => value >= 1101 && value <= 1200, new PellAmounts(4945, 3709, 2473, 1236) },
            { value => value >= 1201 && value <= 1300, new PellAmounts(4845, 3634, 2423, 1211) },
            { value => value >= 1301 && value <= 1400, new PellAmounts(4745, 3559, 2373, 1186) },
            { value => value >= 1401 && value <= 1500, new PellAmounts(4645, 3484, 2323, 1161) },
            { value => value >= 1501 && value <= 1600, new PellAmounts(4545, 3409, 2273, 1136) },
            { value => value >= 1601 && value <= 1700, new PellAmounts(4445, 3334, 2223, 1111) },
            { value => value >= 1701 && value <= 1800, new PellAmounts(4345, 3259, 2173, 1086) },
            { value => value >= 1801 && value <= 1900, new PellAmounts(4245, 3184, 2123, 1061) },
            { value => value >= 1901 && value <= 2000, new PellAmounts(4145, 3109, 2073, 1036) },
            { value => value >= 2001 && value <= 2100, new PellAmounts(4045, 3034, 2023, 1011) },
            { value => value >= 2101 && value <= 2200, new PellAmounts(3945, 2959, 1973, 986) },
            { value => value >= 2201 && value <= 2300, new PellAmounts(3845, 2884, 1923, 961) },
            { value => value >= 2301 && value <= 2400, new PellAmounts(3745, 2809, 1873, 936) },
            { value => value >= 2401 && value <= 2500, new PellAmounts(3645, 2734, 1823, 911) },
            { value => value >= 2501 && value <= 2600, new PellAmounts(3545, 2659, 1773, 886) },
            { value => value >= 2601 && value <= 2700, new PellAmounts(3445, 2584, 1723, 861) },
            { value => value >= 2701 && value <= 2800, new PellAmounts(3345, 2509, 1673, 836) },
            { value => value >= 2801 && value <= 2900, new PellAmounts(3245, 2434, 1623, 811) },
            { value => value >= 2901 && value <= 3000, new PellAmounts(3145, 2359, 1573, 786) },
            { value => value >= 3001 && value <= 3100, new PellAmounts(3045, 2284, 1523, 761) },
            { value => value >= 3101 && value <= 3200, new PellAmounts(2945, 2209, 1473, 736) },
            { value => value >= 3201 && value <= 3300, new PellAmounts(2845, 2134, 1423, 711) },
            { value => value >= 3301 && value <= 3400, new PellAmounts(2745, 2059, 1373, 686) },
            { value => value >= 3401 && value <= 3500, new PellAmounts(2645, 1984, 1323, 661) },
            { value => value >= 3501 && value <= 3600, new PellAmounts(2545, 1909, 1273, 636) },
            { value => value >= 3601 && value <= 3700, new PellAmounts(2445, 1834, 1223, 611) },
            { value => value >= 3701 && value <= 3800, new PellAmounts(2345, 1759, 1173, 0) },
            { value => value >= 3801 && value <= 3900, new PellAmounts(2245, 1684, 1123, 0) },
            { value => value >= 3901 && value <= 4000, new PellAmounts(2145, 1609, 1073, 0) },
            { value => value >= 4001 && value <= 4100, new PellAmounts(2045, 1534, 1023, 0) },
            { value => value >= 4101 && value <= 4200, new PellAmounts(1945, 1459, 973, 0) },
            { value => value >= 4201 && value <= 4300, new PellAmounts(1845, 1384, 923, 0) },
            { value => value >= 4301 && value <= 4400, new PellAmounts(1745, 1309, 873, 0) },
            { value => value >= 4401 && value <= 4500, new PellAmounts(1645, 1234, 823, 0) },
            { value => value >= 4501 && value <= 4600, new PellAmounts(1545, 1159, 773, 0) },
            { value => value >= 4601 && value <= 4700, new PellAmounts(1445, 1084, 723, 0) },
            { value => value >= 4701 && value <= 4800, new PellAmounts(1345, 1009, 673, 0) },
            { value => value >= 4801 && value <= 4900, new PellAmounts(1245, 934, 623, 0) },
            { value => value >= 4901 && value <= 5000, new PellAmounts(1145, 859, 0, 0) },
            { value => value >= 5001 && value <= 5100, new PellAmounts(1045, 784, 0, 0) },
            { value => value >= 5101 && value <= 5200, new PellAmounts(945, 709, 0, 0) },
            { value => value >= 5201 && value <= 5300, new PellAmounts(845, 634, 0, 0) },
            { value => value >= 5301 && value <= 5400, new PellAmounts(745, 0, 0, 0) },
            { value => value >= 5401 && value <= 5486, new PellAmounts(652, 0, 0, 0) },
            { value => value >= 5487, new PellAmounts(0, 0, 0, 0) },


        };

        private readonly Dictionary<string, int> RegionForState = new Dictionary<string, int>()
        {
            { "AK", 1 }, { "AL", 7 }, { "AR", 5 }, { "AZ", 1 }, { "CA", 3 },
            { "CO", 1 }, { "CT", 4 }, { "DC", 8 }, { "DE", 6 }, { "FL", 7 },
            { "GA", 7 }, { "HI", 3 }, { "IA", 2 }, { "ID", 1 }, { "IL", 2 },
            { "IN", 2 }, { "KS", 2 }, { "KY", 7 }, { "LA", 7 }, { "MA", 4 },
            { "MD", 8 }, { "ME", 4 }, { "MI", 2 }, { "MN", 2 }, { "MO", 2 },
            { "MS", 7 }, { "MT", 1 }, { "NC", 7 }, { "ND", 2 }, { "NE", 2 },
            { "NH", 4 }, { "NJ", 6 }, { "NM", 5 }, { "NV", 1 }, { "NY", 6 },
            { "OH", 2 }, { "OK", 5 }, { "OR", 4 }, { "PA", 2 }, { "PR", 6 },
            { "RI", 4 }, { "SC", 7 }, { "SD", 2 }, { "TN", 7 }, { "TX", 5 },
            { "UT", 1 }, { "VA", 8 }, { "VI", 7 }, { "VT", 4 }, { "WA", 4 },
            { "WI", 2 }, { "WV", 8 }, { "WY", 1 }
        };

        private readonly Dictionary<int, CLE> WithParentsCLE1718 = new Dictionary<int, CLE>()
        {
            { 1, new CLE(731, 243, 379) },
            { 2, new CLE(715, 238, 371) },
            { 3, new CLE(770, 257, 399) },
            { 4, new CLE(751, 250, 390) },
            { 5, new CLE(730, 243, 381) },
            { 6, new CLE(766, 256, 397) },
            { 7, new CLE(722, 240, 375) },
            { 8, new CLE(837, 279, 434) },
        };

        private readonly Dictionary<int, CLE> OffCampusCLE1718 = new Dictionary<int, CLE>()
        {
            { 1, new CLE(1090, 364, 566) },
            { 2, new CLE(1067, 357, 553) },
            { 3, new CLE(1148, 385, 601) },
            { 4, new CLE(1121, 373, 582) },
            { 5, new CLE(1090, 363, 565) },
            { 6, new CLE(1143, 381, 593) },
            { 7, new CLE(1079, 360, 560) },
            { 8, new CLE(1249, 416, 648) }
        };

        private readonly Dictionary<int, CLE> WithParentsCLE1819 = new Dictionary<int, CLE>()
        {
            { 1, new CLE(778, 265, 425) },
            { 2, new CLE(809, 275, 443) },
            { 3, new CLE(809, 410, 661) },
            { 4, new CLE(761, 259, 416) },
            { 5, new CLE(765, 260, 625) },
            { 6, new CLE(787, 267, 431) },
            { 7, new CLE(750, 255, 410) },
            { 8, new CLE(824, 280, 451) }
        };

        private readonly Dictionary<int, CLE> OffCampusCLE1819 = new Dictionary<int, CLE>()
        {
            { 1, new CLE(1162, 395, 636) },
            { 2, new CLE(1208, 410, 661) },
            { 3, new CLE(1208, 410, 661) },
            { 4, new CLE(1136, 386, 622) },
            { 5, new CLE(1142, 388, 625) },
            { 6, new CLE(1175, 399, 643) },
            { 7, new CLE(1120, 380, 613) },
            { 8, new CLE(1230, 418, 673) }
        };        

        private readonly Dictionary<int, decimal> SubAmounts = new Dictionary<int, decimal>()
            {
                { 0, 0 },
                { 1, 3500 },
                { 2, 4500 },
                { 3, 5500 },
                { 6, 0 }
            };

        private readonly Dictionary<int, decimal> UnsubAmounts = new Dictionary<int, decimal>()
            {
                { 0, 0 },
                { 1, 6000 },
                { 2, 6000 },
                { 3, 7000 },
                { 6, 20500 }
            };

        private readonly Dictionary<string, double> TermMultipliers = new Dictionary<string, double>()
            {
                { "", 0 },
                { "full-time", 1 },
                { "three-quarter-time", 0.75 },
                { "half-time", 0.5 },
                { "below-half-time", 0.25 }
            };

        private ProcessingDetail GetProcessingDetail(long id)
        {
            ViewBag.id = id;
            ProcessingDetail re = new ProcessingDetail
            {
                Proc = db.Processing.Include(p => p.Queue).Where(p => p.ProcID == id).ToList()[0]
            };
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

            DirectLoanCalc(re);
            PellCalc(re);
            BudgetCalc(re);
            ViewBag.FinalLoanPeriod = (re.Rec.IsProratedLoan || ViewBag.SulaLoanPeriod == "4 Month" || ViewBag.ExistingLoanPeriod == "4 Month") ? "4 Month" : "8 Month";

            return re;
        }

        public ActionResult Calculate(ProcessingDetail detail, string mainReturn)
        {
            Record record = db.Record.Find(detail.Rec.RecordID);
            record.DependencyStatus = detail.Rec.DependencyStatus;
            record.AcademicYear = detail.Rec.AcademicYear;
            record.SubAgg = detail.Rec.SubAgg;
            record.CombinedAgg = detail.Rec.CombinedAgg;
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
                return Redirect(Url.Action("OpenAdmin", "FileOpen", new { id, ViewBag.mainReturn }));
            }
            ViewBag.OpenReturn = Url.Action("OpenFile", "FileOpen", new { id, ViewBag.mainReturn });
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
            ViewBag.OpenReturn = Url.Action("OpenAdmin", "FileOpen", new { id, ViewBag.mainReturn });
            ProcessingDetail re = GetProcessingDetail(id);
            
            return View(re);
        }
        #region ProcessingFile

        private void DirectLoanCalc(ProcessingDetail processingDetail)
        {
            Record record = processingDetail.Rec;
            int AcademicYear = Convert.ToInt32(record.AcademicYear);
            decimal NumCredits = Convert.ToDecimal(record.NumCredits);
            decimal SubAgg = Convert.ToDecimal(record.SubAgg);
            decimal CombineAgg = Convert.ToDecimal(record.CombinedAgg);
            decimal SubUsed = Convert.ToDecimal(record.SubAmountUsed);
            decimal UnsubUsed = Convert.ToDecimal(record.UnsubAmountUsed);

            decimal StartingAYFunding1 = (record.DependencyStatus == "DependentNoParentInfo") ? 0 : SubAmounts[AcademicYear];
            decimal StartingAYFunding2 = 2000;
            if (record.DependencyStatus == "Independent" || record.DependencyStatus == "DependentOverride")
            {
                StartingAYFunding2 = UnsubAmounts[AcademicYear];
            }
            else if (AcademicYear == 6)
            {
                StartingAYFunding2 = 20500;
            }

            decimal ProratedAmount1 = (record.IsProratedLoan) ? Math.Round(NumCredits / 36 * StartingAYFunding1) : StartingAYFunding1;
            decimal ProratedAmount2 = (record.IsProratedLoan) ? Math.Round(NumCredits / 36 * StartingAYFunding2) : StartingAYFunding2;

            decimal SulaAdjustment1;
            decimal SulaAdjustment2;
            string SulaLoanPeriod = "8 Month";
            switch(SulaCalc(record, StartingAYFunding1, StartingAYFunding2))
            {
                case "ALL":
                    SulaAdjustment1 = StartingAYFunding1;
                    SulaAdjustment2 = StartingAYFunding2;
                    break;
                case "HALF":
                    SulaAdjustment1 = StartingAYFunding1 / 2;
                    SulaAdjustment2 = StartingAYFunding2 / 2;
                    SulaLoanPeriod = "4 Month";
                    break;
                default:
                    SulaAdjustment1 = 0;
                    SulaAdjustment2 = StartingAYFunding1 = StartingAYFunding2;
                    break;
            }

            decimal ProrateSulaSwitch1 = (record.IsProratedLoan) ? Math.Min(ProratedAmount1, SulaAdjustment1) : SulaAdjustment1;
            decimal ProrateSulaSwitch2 = (record.IsProratedLoan) ? Math.Min(ProratedAmount2, SulaAdjustment2) : SulaAdjustment2;

            decimal RemainingAgg1 = ((AcademicYear == 6) ? 65500 : (record.DependencyStatus == "Independent" || record.DependencyStatus == "DependentOverride") ? 23000 : 23000) - SubAgg;
            decimal RemainingAgg2 = ((AcademicYear == 6) ? 138500 : (record.DependencyStatus == "Independent" || record.DependencyStatus == "DependentOverride") ? 57500 : 31000) - CombineAgg;

            decimal SubAggRealloc1 = (RemainingAgg2 < 0) ? 0 : Math.Min(ProrateSulaSwitch1, RemainingAgg1);
            decimal SubAggRealloc2 = (SubAggRealloc1 == RemainingAgg1) ? ProrateSulaSwitch2 + (ProrateSulaSwitch1 - RemainingAgg1) : ProrateSulaSwitch2;

            decimal UnsubAggRealloc1 = SubAggRealloc1;
            decimal UnsubAggRealloc2 = Math.Min(SubAggRealloc2, RemainingAgg2 - UnsubAggRealloc1);

            decimal ExistingLoanRemaining1 = UnsubAggRealloc1 - SubUsed;
            decimal ExistingLoanRemaining2 = UnsubAggRealloc2 - UnsubUsed;

            decimal SubExistingRealloc1 = Math.Min(ExistingLoanRemaining1, UnsubAggRealloc1);
            decimal SubExistingRealloc2 = Math.Min(ExistingLoanRemaining2, UnsubAggRealloc2);

            decimal UnsubExistingRealloc1 = SubExistingRealloc1;
            decimal UnsubExistingRealloc2;
            if (UnsubAggRealloc1 - SubExistingRealloc2 > 0 && ExistingLoanRemaining2 - SubExistingRealloc2 > 0)
            {
                UnsubExistingRealloc2 = Math.Min(ExistingLoanRemaining2, UnsubAggRealloc1 - SubExistingRealloc1 + SubExistingRealloc2);
            }
            else
            {
                UnsubExistingRealloc2 = SubExistingRealloc2;
            }

            decimal FinalResult1 = Math.Max(0, UnsubExistingRealloc1);
            decimal FinalResult2 = Math.Max(0, UnsubExistingRealloc2);

            decimal StartingSub = StartingAYFunding1;
            decimal StartingUnsub = StartingAYFunding2;            

            // Now assign to the appropriate view variables
            ViewBag.StartingSub = String.Format(DecimalAmountFormat, StartingAYFunding1);
            ViewBag.StartingUnsub = String.Format(DecimalAmountFormat, StartingAYFunding2);
            ViewBag.AvailableAggSub = String.Format(DecimalAmountFormat, RemainingAgg1);
            ViewBag.AvailableAggCombine = String.Format(DecimalAmountFormat, RemainingAgg2);
            ViewBag.MaxProrateSub = String.Format(DecimalAmountFormat, ProratedAmount1);
            ViewBag.MaxProrateUnsub = String.Format(DecimalAmountFormat, ProratedAmount2);
            ViewBag.MaxRemainingSub = String.Format(DecimalAmountFormat, StartingSub - SubUsed);
            ViewBag.MaxRemainingUnsub = String.Format(DecimalAmountFormat, StartingUnsub - UnsubUsed);
            ViewBag.MaxSulaSub = String.Format(DecimalAmountFormat, SulaAdjustment1);
            ViewBag.MaxSulaUnsub = String.Format(DecimalAmountFormat, SulaAdjustment2);
            ViewBag.ExistingLoanPeriod = (record.ExistingAYEndsBeforeTermTwo == true) ? "4 Month" : "8 Month";
            ViewBag.SulaLoanPeriod = SulaLoanPeriod;
            ViewBag.FinalMaxSub = String.Format(DecimalAmountFormat, FinalResult1);
            ViewBag.FinalMaxUnsub = String.Format(DecimalAmountFormat, FinalResult2);
        }

        // TODO: Fix Pell Amount based on EFC range
        private void PellCalc(ProcessingDetail processingDetail)
        {
            Record record = processingDetail.Rec;
            decimal LEU = Convert.ToDecimal(record.LEU);
            decimal PercentPellUsed = Convert.ToDecimal(record.PercentPellUsed) / 100;
            decimal EFC = Convert.ToDecimal(record.EFC);
            int AcademicYear = Convert.ToInt32(record.AcademicYear);

            var key = PellByEFC.Keys.Single(efc => efc(EFC));
            var Pell = PellByEFC[key];
            decimal MaxPellAY;

            switch(record.StatusTermOne)
            {
                case "full-time":
                    MaxPellAY = Pell.FullTime;
                    break;
                case "three-quarter-time":
                    MaxPellAY = Pell.ThreeQuarterTime;
                    break;
                case "half-time":
                    MaxPellAY = Pell.HalfTime;
                    break;
                case "below-half-time":
                    MaxPellAY = Pell.BelowHalfTime;
                    break;
                default:
                    MaxPellAY = 0;
                    break;
            }
            
            decimal AdditionalPell = MaxPellAY / 2;
            decimal RemainingLEU = (6 - (LEU / 100)) * 100;
            decimal MaxPellLEU = (RemainingLEU < (decimal)1.5) ? MaxPellAY * RemainingLEU : MaxPellAY;
            decimal MaxExistingPct = (decimal)1.5 - (PercentPellUsed);
            decimal MaxExistingPell = Math.Floor(MaxPellAY * MaxExistingPct);

            decimal FinalMaxPct = Math.Min((decimal)1.5, RemainingLEU);
            FinalMaxPct = Math.Min(FinalMaxPct, MaxExistingPct);

            decimal FinalMaxAmountTotal;
            if (record.DependencyStatus == "DependentNoParentInfo" || AcademicYear == 6)
            {
                FinalMaxAmountTotal = 0;
            }
            else
            {
                decimal temp = (FinalMaxPct >= 1) ? MaxPellAY : FinalMaxPct * MaxPellAY;
                decimal temp2 = (1 - FinalMaxPct < 0) ? (1 - FinalMaxPct) * MaxPellAY * -1 : 0;

                FinalMaxAmountTotal = Math.Floor(temp) + Math.Floor(temp2);
            }

            string StatusTermOne = record.StatusTermOne ?? "";
            string StatusTermTwo = record.StatusTermTwo ?? "";
            string StatusTermThree = record.StatusTermThree ?? "";

            decimal FinalMaxAmountTerm1 = Math.Min(FinalMaxAmountTotal, Math.Ceiling(MaxPellAY * ((decimal)TermMultipliers[StatusTermOne] / 2)));
            decimal FinalMaxAmountTerm2 = Math.Min(FinalMaxAmountTotal - FinalMaxAmountTerm1, Math.Floor(MaxPellAY * ((decimal)TermMultipliers[StatusTermTwo] / 2)));
            decimal FinalMaxAmountTerm3 = Math.Min(FinalMaxAmountTotal - FinalMaxAmountTerm1 - FinalMaxAmountTerm2, Math.Floor(MaxPellAY * ((decimal)TermMultipliers[StatusTermThree] / 2)));

            decimal FinalMaxPctTerm1 = (MaxPellAY > 0) ? FinalMaxAmountTerm1 / MaxPellAY : 0;
            decimal FinalMaxPctTerm2 = (MaxPellAY > 0) ? FinalMaxAmountTerm2 / MaxPellAY : 0;
            decimal FinalMaxPctTerm3 = (MaxPellAY > 0) ? FinalMaxAmountTerm3 / MaxPellAY : 0;

            // Assign to view variables
            ViewBag.MaxPellAY = String.Format(DecimalAmountFormat, MaxPellAY);
            ViewBag.AdditionalPell = String.Format(DecimalAmountFormat, AdditionalPell);
            ViewBag.MaxPellLEU = String.Format(DecimalAmountFormat, MaxPellLEU);
            ViewBag.RemainingLEU = String.Format(DecimalPercentFormat, RemainingLEU);
            ViewBag.MaxExistingPell = String.Format(DecimalAmountFormat, MaxExistingPell);
            ViewBag.MaxExistingPct = String.Format(DecimalPercentFormat, MaxExistingPct * 100);
            ViewBag.FinalMaxAmount = String.Format(DecimalAmountFormat, FinalMaxAmountTotal);
            ViewBag.FinalMaxPct = String.Format(DecimalPercentFormat, FinalMaxPct * 100);
            ViewBag.FinalMaxAmountTerm1 = String.Format(DecimalAmountFormat, FinalMaxAmountTerm1);
            ViewBag.FinalMaxAmountTerm2 = String.Format(DecimalAmountFormat, FinalMaxAmountTerm2);
            ViewBag.FinalMaxAmountTerm3 = String.Format(DecimalAmountFormat, FinalMaxAmountTerm3);
            ViewBag.FinalMaxPctTerm1 = String.Format(DecimalPercentFormat, FinalMaxPctTerm1 * 100);
            ViewBag.FinalMaxPctTerm2 = String.Format(DecimalPercentFormat, FinalMaxPctTerm2 * 100);
            ViewBag.FinalMaxPctTerm3 = String.Format(DecimalPercentFormat, FinalMaxPctTerm3 * 100);
        }

        // TODO: Add CLEs for '17-'18
        private void BudgetCalc(ProcessingDetail processingDetail)
        {
            Record record = processingDetail.Rec;

            decimal Room = 0;
            decimal Travel = 0;
            decimal Personal = 0;
            decimal Tuition;

            decimal NumMonthsInAY = Convert.ToDecimal(record.NumMonthsInAY);

            if (!String.IsNullOrEmpty(record.StateOnISIR))
            {
                int region = RegionForState[record.StateOnISIR];

                CLE cle;
                if (record.AwardYear == "'17-'18")
                    cle = (record.IsWithParents == true) ? WithParentsCLE1718[region] : OffCampusCLE1718[region];
                else
                    cle = (record.IsWithParents == true) ? WithParentsCLE1819[region] : OffCampusCLE1819[region];

                Room = cle.Room * NumMonthsInAY;
                Travel = (record.IsOnlineStudent == true) ? 0 : cle.Travel * NumMonthsInAY;
                Personal = cle.Personal * NumMonthsInAY;
            }

            decimal NumEstimatedCredits = Convert.ToDecimal(record.NumEstimatedCredits);
            decimal CostPerCredit = Convert.ToDecimal(record.CostPerCredit);

            Tuition = NumEstimatedCredits * CostPerCredit;

            // Assign to view variables
            ViewBag.Room = String.Format(DecimalAmountFormat, Room);
            ViewBag.Travel = String.Format(DecimalAmountFormat, Travel);
            ViewBag.Personal = String.Format(DecimalAmountFormat, Personal);
            ViewBag.Tuition = String.Format(DecimalAmountFormat, Tuition);
        }

        public string SulaCalc(Record record, decimal StartingSub, decimal StartingUnsub)
        {
            string SulaResult = String.Empty;

            if (StartingSub == 0)
                return "ALL";

            double Term1Usage = record.AttendanceTermOne != null ? TermMultipliers[record.AttendanceTermOne] / 2 : 0;
            double Term2Usage = record.AttendanceTermTwo != null ? TermMultipliers[record.AttendanceTermTwo] / 2 : 0;
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
                    Processing newProcessing = new Processing
                    {
                        InFilingCabinet = false,
                        ProcessingError = null,
                        ProcInQueue = DateTime.Now,
                        ProcToUser = null,
                        ProcUserComplete = null,
                        QueueID = student.Queue.QueueNextQueue.Value,
                        RecordID = student.RecordID,
                        Username = null
                    };
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
            Note note = new Note
            {
                RecordID = i.RecordID,
                Username = i.Username
            };
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
                ViewBag.Username = new SelectList(db.User.Where(u => u.IsActive), "Username", "Username");
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
            Note note = new Note
            {
                Username = User.Identity.Name,
                RecordID = processing.RecordID,
                Note1 = "Reassigned to " + noteString
            };
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

            ViewBag.Username = new SelectList(db.User.Where(u => u.IsActive), "Username", "Username");
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
            ProcessingError i = new ProcessingError
            {
                ProcID = id
            };
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
            return RedirectToAction("OpenAdmin", new { id = returnID, mainReturn });
        }
        #endregion


    }

}