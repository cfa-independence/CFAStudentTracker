using CFAStudentTracker.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models
{
    public class ProcessingList
    {
        public List<ProcessingVM> Setup(List<Processing> procList)
        {
            List<ProcessingVM> vm = new List<ProcessingVM>();
            foreach (var item in procList)
            {
                ProcessingVM v = new ProcessingVM();
                if (item.Record.ProcPriority)
                {
                    v.status = "Priority";
                }
                else if (item.ProcToUser == null)
                {
                    v.status = "In Queue";
                }
                else if (item.InFilingCabinet)
                {
                    v.status = "In Cabinet";
                }
                else if (item.ProcUserComplete != null)
                {
                    v.status = "Completed";
                }
                else
                {
                    v.status = "In User Processing";
                }
                v.p = item;
                vm.Add(v);
            }

            return vm;
        }
    }
}