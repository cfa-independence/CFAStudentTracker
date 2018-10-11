using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models.ViewModels
{
    public class QueueVM
    {
        public Queue q { get; set; }
        public int FilesInQueue { get; set; }
        public int UsersInQueue { get; set; }
        public double AvgQueueTime { get; set; }
        public double OldestFileInQueue { get; set; }
        public string NextQueue { get; set; }
        public bool Auditing { get; set; }
        public string OldestProcDate { get; set; }
    }
}