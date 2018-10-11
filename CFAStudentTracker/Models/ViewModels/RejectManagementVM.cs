using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CFAStudentTracker.Models;
using CFAStudentTracker.Models.ViewModels;

namespace CFAStudentTracker.Models
{
    public class RejectManagementVM
    {
        private CFAEntities db = new CFAEntities();
        public List<UserStat> UserErrors { get; set; }
        public IEnumerable<ProcessingError> ErrorList { get; set; }
        public double HighestError { get; set; }
        public short? groupid { get; set; }

        public RejectManagementVM(short? id)
        {
            UserErrors = new List<UserStat>();
            groupid = id;
            ErrorList = db.ProcessingError.Include(p => p.ErrorComplete).Include(p => p.ErrorType).Include(p => p.Processing).Include(p => p.Processing.Queue).Where(p => p.DateComplete == null && p.Processing.Username != null &&p.Processing.Queue.GroupID == groupid);
            var GroupCount = ErrorList.Where(p => p.ProcID != null).GroupBy(p => new { p.Processing.Username, p.ErrorType.Description }).Select(p=> new SSAndInt { secondString = p.Key.Username.ToString(), MyString = p.Key.Description, MyInt = (1.0*p.Count()) });
            
            if (GroupCount.ToList().Count != 0)
            {
                var users = GroupCount.Select(u => u.secondString).Distinct().ToList();
                HighestError = GroupCount.Select(s => s.MyInt).Max();
                foreach (var item in users)
                {
                    var u = new UserStat();
                    u.user = db.User.Where(u1 => u1.Username == item).First();
                    u.typeCount = GroupCount.Where(g => g.secondString == item).ToList<StringAndInt>();
                    u.processedToday = (int)u.typeCount.Select(t => t.MyInt).Sum();
                    //foreach (var i1 in filter)
                    //{
                    //    var si = new StringAndInt();
                    //    si.MyString = i1.Type;
                    //    si.MyInt = i1.MyInt;
                    //    u.typeCount.Add(si);
                    //}
                    UserErrors.Add(u);
                }
            }
           
            var u2 = new UserStat();
            var user = new User();
            user.Username = "Not Found";
            u2.user = user;
            u2.typeCount = ErrorList.Where(p => p.ProcID == null).GroupBy(p => new { p.ErrorType.Description }).Select(p => new StringAndInt { MyString = p.Key.Description, MyInt = (1.0 * p.Count()) }).ToList();
            u2.processedToday = (int)u2.typeCount.Select(t => t.MyInt).Sum();
            if (u2.typeCount.Count != 0)
            {
                UserErrors.Add(u2);
            }
            
            //Parse users and amount of type of rejects
        }
        
    }
}