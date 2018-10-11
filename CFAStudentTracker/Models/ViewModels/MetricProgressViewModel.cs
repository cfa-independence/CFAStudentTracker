using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace CFAStudentTracker.Models
{
    public class MetricProgressViewModel
    {
        private CFAEntities db = new CFAEntities();

        public string username { get; set; }
        public List<StringAndInt> FilesProcessed14Day { get; set; }
        public List<StringAndInt> FilesProcessed28Day { get; set; }
        public List<StringAndInt> FilesProcessed56Day { get; set; }
        public List<StringAndInt> error14 { get; set; }
        public List<StringAndInt> error28 { get; set; }
        public List<StringAndInt> error56 { get; set; }
        public List<StringAndInt> nerror14 { get; set; }
        public List<StringAndInt> nerror28 { get; set; }
        public List<StringAndInt> nerror56 { get; set; }
        public List<StringAndInt> notComplete { get; set; }
        public double PerHour14 { get; set; }
        public double PerHour28 { get; set; }
        public double PerHour56 { get; set; }
        public double ErrPercent14 { get; set; }
        public double ErrPercent28 { get; set; }
        public double ErrPercent56 { get; set; }
        public DateTime LastUpdated { get; set; }
        public MetricProgressViewModel()
        {
        }

        public MetricProgressViewModel(string username)
        {
            
            this.username = username;
            if(db.Hour.Where(p => p.Username == username).Count() == 0)
            {
                FilesProcessed14Day = new List<StringAndInt>();
                FilesProcessed28Day = new List<StringAndInt>();
                FilesProcessed56Day = new List<StringAndInt>();
                var xxx = new StringAndInt();
                xxx.MyString = "N/A";
                xxx.MyInt = 0;
                error14 = new List<StringAndInt>();
                error14.Add(xxx);
                error28 = new List<StringAndInt>();
                error28.Add(xxx);
                error56 = new List<StringAndInt>();
                error56.Add(xxx);
                nerror14 = new List<StringAndInt>();
                nerror28 = new List<StringAndInt>();
                nerror56 = new List<StringAndInt>();
                notComplete = new List<StringAndInt>();
                PerHour14 = 0;
                PerHour28 = 0;
                PerHour56 = 0;
                ErrPercent14 = 0;
                ErrPercent28 = 0;
                ErrPercent56 = 0;
                LastUpdated = new DateTime();
                return;
            }
            LastUpdated = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate;
            PerHour14 = 0;
            PerHour28 = 0;
            PerHour56 = 0;

            #region FilesProcessed
            FilesProcessed14Day = FindFiles(14);
            PerHour14 = WeightedAverageFile(FilesProcessed14Day,14);
            FilesProcessed28Day = FindFiles(28);
            PerHour28 = WeightedAverageFile(FilesProcessed28Day, 28);
            FilesProcessed56Day = FindFiles(56);
            PerHour56 = WeightedAverageFile(FilesProcessed56Day, 56);
            #endregion
            
            #region Errors On Files
            error14 = FindErrors(14);
            error28 = FindErrors(28);
            error56 = FindErrors(56);
            nerror14 = FindNotAtFault(14);
            nerror28 = FindNotAtFault(28);
            nerror56 = FindNotAtFault(56);
            notComplete = FindNotComplete();
            #endregion
            #region Overview
            
            ErrPercent14 = FilesProcessed14Day.Sum(p=>p.MyInt);
            ErrPercent28 = FilesProcessed28Day.Sum(p => p.MyInt);
            ErrPercent56 = FilesProcessed56Day.Sum(p => p.MyInt);
            if (ErrPercent14 > 0)
            {
                ErrPercent14 = error14.Last().MyInt / ErrPercent14 *100;
            }
            if (ErrPercent28 > 0)
            {
                ErrPercent28 = error28.Last().MyInt / ErrPercent28 * 100;
            }
            if (ErrPercent56 > 0)
            {
                ErrPercent56 = error56.Last().MyInt / ErrPercent56 * 100;
            }
            #endregion

        }
        public int FoundErrors(int days)
        {
            var backDate = DateTime.Today.AddDays(-days);
            var ErrorFound = db.ProcessingError.Include(p => p.Processing).Include(p => p.ErrorComplete).Where(p => p.Processing.Username == username && p.ErrorComplete.Counted).Select(p => new { p.Processing.ProcID, p.ErrorType.Description, p.DateComplete });
            var xxxx = ErrorFound.GroupBy(p => p.ProcID).Select(g => new { g.Key, MyInt = g.Min(p => p.DateComplete) });
            return xxxx.Where(p => p.MyInt >= backDate).Count();

        }
        public double WeightedAverageFile(List<StringAndInt> typesCount, int days)
        {
            DateTime FirstDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(-days);
            DateTime LastDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(1);
            var hoursWorkedList = db.Hour.Where(p => p.HourDate >= FirstDate && p.HourDate < LastDate && p.Username == username);
            double hoursWorked = 0;
            if(hoursWorkedList.Count() != 0)
            {
                hoursWorked = (double)hoursWorkedList.Sum(p => p.HourAmount);
            }
            Helpers h = new Helpers();
            double totalWeightedFiles = h.WeightedFileAmount(FirstDate,LastDate, username);

            var weightedAvg = totalWeightedFiles / hoursWorked;

            return weightedAvg;
        }
        public List<StringAndInt> FindFiles(int days)
        {
            DateTime FirstDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(-days);
            DateTime LastDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(1);
            var processed = db.Processing.Include(p => p.Record).Where(p => p.Username == username && p.ProcUserComplete >= FirstDate && p.ProcUserComplete < LastDate);
            IQueryable<StringAndInt> r = from pro in processed
                                         group pro by pro.Record.FileType.TypeDescription into proGroup
                                         orderby proGroup.Key
                                         select new StringAndInt { MyString = proGroup.Key, MyInt = proGroup.Count() };
            var re = r.ToList();
            return re;


        }
        private List<StringAndInt> FindErrors(int days)
        {
            DateTime FirstDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(-days);
            DateTime LastDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(1);
            var processed = db.ProcessingError.Include(p => p.Processing).Include(p => p.ErrorComplete).Where(p => p.Processing.Username == username && p.DateComplete >= FirstDate && p.DateComplete < LastDate && p.ErrorComplete.Counted);
            IQueryable<StringAndInt> r = from err in processed
                                         group err by err.ErrorType.Description into errGroup
                                         orderby errGroup.Key
                                         select new StringAndInt { MyString = errGroup.Key, MyInt = errGroup.Count() };
            var re = r.ToList();
            StringAndInt i = new StringAndInt();
            i.MyString = "Total Rejected";
            i.MyInt = processed.Select(p=>p.Processing).Distinct().Count();
            re.Add(i);
            i.MyString = "Total Distinct Rejected";
            i.MyInt = FoundErrors(days);
            re.Add(i);
            return re;

                     
        }
        private List<StringAndInt> FindNotComplete()
        {
            var processed = db.ProcessingError.Include(p => p.Processing).Include(p => p.ErrorComplete).Where(p => p.Processing.Username == username && p.DateComplete == null);
            IQueryable<StringAndInt> r = from err in processed
                                         group err by err.ErrorType.Description into errGroup
                                         orderby errGroup.Key
                                         select new StringAndInt { MyString = errGroup.Key, MyInt = errGroup.Count() };
            var re = r.ToList();
            StringAndInt i = new StringAndInt();
            i.MyString = "Total Errors Not Completed";
            i.MyInt = processed.Select(p => p.Processing).Distinct().Count();
            re.Add(i);
            return re;


        }
        private List<StringAndInt> FindNotAtFault(int days)
        {
            DateTime FirstDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(-days);
            DateTime LastDate = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(1);
            var dtdays = db.Hour.Where(p => p.Username == username).OrderByDescending(p => p.HourDate).First().HourDate.AddDays(-days);
            var processed = db.ProcessingError.Include(p => p.Processing).Include(p => p.ErrorComplete).Where(p => p.Processing.Username == username && p.DateComplete >= FirstDate && p.DateComplete < LastDate  && !p.ErrorComplete.Counted);
            IQueryable<StringAndInt> r = from err in processed
                                         group err by err.ErrorComplete.Description into errGroup
                                         orderby errGroup.Key
                                         select new StringAndInt { MyString = errGroup.Key, MyInt = errGroup.Count() };
            var re = r.ToList();
            StringAndInt i = new StringAndInt();
            i.MyString = "Total Not At Fault";
            i.MyInt = processed.Select(p => p.Processing).Distinct().Count();
            re.Add(i);
            return re;


        }
    }
}