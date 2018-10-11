using CFAStudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CFAStudentTracker.Models.ViewModels
{
    public class SystemManagementVM
    {
        private CFAEntities db = new CFAEntities();
        public List<ProcessingError> ErrorsNotAssigned { get; set; }
        public List<ProcessingError> ErrorsWithNull { get; set; }
        public List<StringAndInt> DuplicateSSN { get; set; }
        public SystemManagementVM()
        {
            ErrorsNotAssigned = new List<ProcessingError>();
            ErrorsWithNull = new List<ProcessingError>();
            var NotCompleteErrors = db.ProcessingError.Include(p => p.Processing).Where(p => p.DateComplete == null);
            var NotInTeam = db.User.Where(p => p.Queue.Count == 0);
            foreach (var item in NotCompleteErrors)
            {
                if (item.ProcID == null)
                {
                    ErrorsWithNull.Add(item);
                } else
                {
                    var NotFound = NotInTeam.Where(p => p.Username == item.Processing.Username).Count();
                    if (NotFound > 0)
                    {

                        ErrorsNotAssigned.Add(item);
                    }
                }
                
                
            }

            var x = db.StudentFile.GroupBy(p => p.FileSSN).Select(p => new StringAndInt { MyInt = p.Count(), MyString = p.Key });

            DuplicateSSN = x.Where(p => p.MyInt > 1).ToList();
        }
    }
    
}