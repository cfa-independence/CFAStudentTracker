using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;


namespace CFAStudentTracker.Models.ViewModels
{
    public class GroupManagementVM
    {
        public short? id { get; set; }
        public IEnumerable<QueueVM> Queues { get; set; }
        public List<GroupManagementUserVM> UserManage { get; set; }
        private CFAEntities db = new CFAEntities();
        public GroupManagementVM(short id)
        {
            this.id = id;
            setQueues(id);
            setUserVM(id);
        }

        private void setUserVM(short id)
        {                                                
            UserManage = new List<GroupManagementUserVM>();
            var queues = db.Queue.Include(p=>p.User).Where(p => p.GroupID == id);
            List<User> Allusers = new List<User>();
            foreach (var item in queues)
            {
                Allusers.AddRange(item.User);
            }
            var users = Allusers.Distinct().OrderBy(u => u.Username);
            var e = new RejectManagementVM(id);
            foreach (var item in users)
            {
                if (item.IsActive)
                {
                    UserStat i = new UserStat(item);
                    GroupManagementUserVM x = new GroupManagementUserVM
                    {
                        User = item.Username,
                        ProcessedToday = i.processedToday,
                        OutstandingErrors = e.ErrorList.Where(p => p.Processing.Username == item.Username).Count()
                    };

                    UserManage.Add(x);
                }
            }            
        }

        private void setQueues(short id)
        {
            DataTable secondT = new DataTable();
            var queue = db.Queue.Include(q => q.Group).Include(q => q.Queue2).Include(q => q.QueueOrder).Where(q => q.GroupID == id);
            List<QueueVM> vm = new List<QueueVM>();
            foreach (var item in queue)
            {
                QueueVM v = new QueueVM();
                if ((db.Queue.Where(p => p.QueueID == item.QueueNextQueue)).Select(q => q.QueueDescription).Count() > 0)
                {
                    v.NextQueue = (db.Queue.Where(p => p.QueueID == item.QueueNextQueue)).Select(q => q.QueueDescription).First();
                }
                if (db.Processing.Where(p => p.ProcUserComplete == null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.ProcInQueue).Count() > 0)
                {
                    v.OldestFileInQueue = Math.Round((double)(DateTime.Today - (db.Processing.Where(p => p.ProcUserComplete == null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.ProcInQueue).First().ProcInQueue)).TotalDays, 2);
                }
                var dodList = db.Processing.Include(path => path.Record).Where(p => p.ProcUserComplete == null && p.Record.DOD != null).Where(p => p.QueueID == item.QueueID).OrderBy(p => p.Record.DOD);
                if (dodList.Count() > 0)
                {
                    v.OldestProcDate = DateTime.Parse(dodList.OrderBy(p => p.Record.DOD).First().Record.DOD.ToString()).ToString("MM/dd/yyyy");
                }

                var fq = db.FilesInQueue(item.QueueID).Sum(p => p.fileAmount).ToString();
                var uq = db.UsersInQueue(item.QueueID).Count().ToString();
                if (fq == "")
                    v.FilesInQueue = 0;
                else
                    v.FilesInQueue = Int32.Parse(fq);
                if (uq == "")
                    v.UsersInQueue = 0;
                else
                    v.UsersInQueue = Int32.Parse(uq);
                var avg = (db.Processing.Where(p => p.ProcUserComplete >= DbFunctions.AddDays(DateTime.Now, -60) && p.Username != null && p.QueueID == item.QueueID).Average(p => DbFunctions.DiffDays(p.ProcInQueue, p.ProcUserComplete)));
                if (avg == null)
                {
                    v.AvgQueueTime = 0;
                }
                else
                {
                    v.AvgQueueTime = Math.Round((double)avg, 2);
                }
                v.Auditing = item.AuditQueue;
                v.q = item;
                vm.Add(v);
            }
            Queues = vm;
        }
    }
}