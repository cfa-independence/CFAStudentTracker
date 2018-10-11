using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace CFAStudentTracker.Models
{
    public class UserStat
    {
        private CFAEntities db = new CFAEntities();

        public User user { get; set; }
        public List<StringAndInt> typeCount { get; set; }
        public int filingCabinet { get; set; }
        public double processedToday { get; set; }

        public UserStat(User user, Queue queue)
        {
            this.user = user;
            typeCount = new List<StringAndInt>();
            filingCabinet = db.Processing.Where(p => p.Username == user.Username && p.ProcUserComplete == null && p.InFilingCabinet == true).Count();
            var data = db.Processing.Include(p => p.Record.FileType).Where(p => p.Username == user.Username && p.ProcUserComplete >= DateTime.Today);
            
            var i = data.Select(p => p.Record.FileType).Distinct().ToList();
            foreach (var item in i)
            {
                var x = new StringAndInt();
                x.MyString = item.TypeDescription;
                x.MyInt = data.Where(p => p.Record.FileTypeID == item.FileTypeID).Count();
                typeCount.Add(x);
            }
            Helpers h = new Helpers();
            
            processedToday = h.WeightedFileAmount(DateTime.Today, DateTime.Today.AddDays(1),user.Username);
        }

        public UserStat(User user)
        {
            this.user = user;
            typeCount = new List<StringAndInt>();
            filingCabinet = db.Processing.Where(p => p.Username == user.Username && p.ProcUserComplete == null && p.InFilingCabinet == true).Count();
            var data = db.Processing.Include(p => p.Record.FileType).Where(p => p.Username == user.Username && p.ProcUserComplete >= DateTime.Today);

            var i = data.Select(p => p.Record.FileType).Distinct().ToList();
            foreach (var item in i)
            {
                var x = new StringAndInt();
                x.MyString = item.TypeDescription;
                x.MyInt = data.Where(p => p.Record.FileTypeID == item.FileTypeID).Count();
                typeCount.Add(x);
            }
            Helpers h = new Helpers();

            processedToday = h.WeightedFileAmount(DateTime.Today, DateTime.Today.AddDays(1), user.Username);
        }
        public UserStat()
        {
            typeCount = new List<StringAndInt>();
        }
    }
    
}