
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models
{
    public class QueueViewModel
    {
        private CFAEntities db = new CFAEntities();

        public Queue queue { get; set; }
        public QueueStat queueStats;
        public List<UserStat> userStats { get; set; }
        public double topUser { get; set; }
        public string SendAuditQueue { get; set; }
        public decimal precentAudit { get; set; }


        public void Setup()
        {
            //var barChart = new BarChart();
            topUser = -1;
            userStats = new List<UserStat>();
            foreach (var item in queue.User)
            {
                UserStat i = new UserStat(item, queue);
                if (i.processedToday > topUser)
                {
                    topUser = i.processedToday;
                }
                userStats.Add(i);

            }
        }
    }

}