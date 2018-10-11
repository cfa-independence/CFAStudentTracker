using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;


namespace CFAStudentTracker.Models
{
    public class IndexFilter
    {
        private CFAEntities db = new CFAEntities();
        public string sStartDate { get; set; }
        public string sEndDate { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string userName { get; set; }

        public IQueryable<Processing> GetCompleteFiles()
        {
            if (startDate == null && endDate == null)
            {
                return db.Processing.Include(p=> p.Record.StudentFile)
                    .Where(p => p.Username == userName && p.ProcUserComplete != null).OrderBy(p=>p.ProcUserComplete);
            } else if (startDate != null && endDate != null)
            {
                return db.Processing.Include(p => p.Record.StudentFile)
                    .Where(p => p.Username == userName && p.ProcUserComplete <= endDate && p.ProcUserComplete >= startDate).OrderBy(p => p.ProcUserComplete);
            }
            else if (startDate == null && endDate != null)
            {
                return db.Processing.Include(p => p.Record.StudentFile)
                    .Where(p => p.Username == userName && p.ProcUserComplete <= endDate).OrderBy(p => p.ProcUserComplete);
            }
            else if (startDate != null && endDate == null)
            {
                return db.Processing.Include(p => p.Record.StudentFile)
                    .Where(p => p.Username == userName && p.ProcUserComplete >= startDate).OrderBy(p => p.ProcUserComplete);
            }
            return null;
        }

    }
}