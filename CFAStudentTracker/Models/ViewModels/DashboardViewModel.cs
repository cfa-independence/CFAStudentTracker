using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace CFAStudentTracker.Models
{
    public class DashboardViewModel
    {
        private CFAEntities db = new CFAEntities();

        public string username { get; set; }
        public List<QueueStat> queueStats;
        public double topUser;
        public MetricProgressViewModel MPVM { get; set; }

        public int errors { get; set; }
        public List<UserStat> userStats { get; set; }


        public DashboardViewModel(string Username)
        {
            MPVM = new MetricProgressViewModel(Username);
            queueStats = new List<QueueStat>();
            topUser = 1;
            username = Username;
            errors = db.ProcessingError.Where(p => p.DateComplete == null && p.Processing.Username == username).Count();
            var queue = db.Queue.Find(1);
            userStats = new List<UserStat>();
            foreach (var item in queue.User)
            {
                UserStat i = new UserStat(item, queue);
                userStats.Add(i);
                if (i.processedToday > topUser)
                {
                    topUser = i.processedToday;
                }
            }
            userStats.OrderBy(u => u.processedToday);
        }
    }
}