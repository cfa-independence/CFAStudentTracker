using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace CFAStudentTracker.Models
{
    public class MasterMPRVM
    {
        private CFAEntities db = new CFAEntities();
        
        public List<MetricProgressViewModel> UserViewModels { get; set; }
        public string QueueName { get; set; }

        public MasterMPRVM(short group)
        {
            QueueName = db.Group.Find(group).GroupName;
            UserViewModels = new List<MetricProgressViewModel>();
            var users = db.Queue.Include(q => q.User).Where(q => q.GroupID == group).Select(p=>p.User);
            foreach (var item in users)
            {
                foreach (var user in item)
                {
                    if(UserViewModels.Where(p=>p.username == user.Username).Count() == 0)
                    {
                        UserViewModels.Add(new MetricProgressViewModel(user.Username));
                    }
                }
                
            }
        }
    }
}