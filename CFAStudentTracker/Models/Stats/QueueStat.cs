using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace CFAStudentTracker.Models
{
    public class QueueStat
    {
        public Queue queue { get; set; }
        public List<StringAndInt> fileTypeAmount { get; set; }
        public int queueAmount { get; set; }
        public double highestTypeAmount { get; set; }
        public int Priorities { get; set; }
        public decimal MyProperty { get; set; }
        public double AvgQueueTime { get; set; }

        private CFAEntities db = new CFAEntities();

        public QueueStat(Queue queue)
        {
            var avgin = (db.Processing.Where(p => p.ProcUserComplete >= DbFunctions.AddDays(DateTime.Now, -60) && p.Username != null).Average(p => DbFunctions.DiffDays(p.ProcInQueue, p.ProcUserComplete))) / 24;
                
            Priorities = db.Processing.Where(p => p.ProcUserComplete == null && p.Record.ProcPriority == true && p.QueueID == queue.QueueID).Count();
            this.queue = queue;
            fileTypeAmount = new List<StringAndInt>();
            var data = db.Processing.Include(p=>p.Record.FileType).Where(p => p.ProcUserComplete == null && p.QueueID == queue.QueueID).ToList();
            queueAmount = data.Count();
            var i = data.Select(p => p.Record.FileType).Distinct().ToList();
            highestTypeAmount = -1;
            foreach (var item in i)
            {
                var x = new StringAndInt();
                x.MyString = item.TypeDescription;
                x.MyInt = data.Where(p => p.Record.FileTypeID == item.FileTypeID).Count();
                if (x.MyInt > highestTypeAmount)
                {
                    highestTypeAmount = x.MyInt;
                }
                fileTypeAmount.Add(x);
            }
            var avg = (db.Processing.Where(p => p.ProcUserComplete >= DbFunctions.AddDays(DateTime.Now, -60) && p.Username != null && p.QueueID == queue.QueueID).Average(p => DbFunctions.DiffDays(p.ProcInQueue, p.ProcUserComplete)));
            if (avg == null)
            {
                AvgQueueTime = 0;
            }
            else
            {
               AvgQueueTime = Math.Round((double)avg, 2);
            }
        }
    }
}