using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace CFAStudentTracker.Models
{
    public class ErrorOpenVM
    {
        private CFAEntities db = new CFAEntities();

        public Processing CurrentProcess { get; set; }
        public IEnumerable<ProcessingError> ProcErrorNotCom { get; set; }
        public IEnumerable<ProcessingError> ProcErrorCom { get; set; }

        public ErrorOpenVM(long? e)
        {
            if (e == null)
            {

            } else
            {
                CurrentProcess = db.Processing.Find(e);
                ProcErrorCom = db.ProcessingError.Include(p=>p.ErrorType).Include(p=>p.ErrorComplete).Where(p => p.ProcID == CurrentProcess.ProcID && p.DateComplete != null);
                ProcErrorNotCom = db.ProcessingError.Include(p => p.ErrorType).Include(p => p.ErrorComplete).Where(p => p.ProcID == CurrentProcess.ProcID && p.DateComplete == null);
            }
            

        }

        

    }
}