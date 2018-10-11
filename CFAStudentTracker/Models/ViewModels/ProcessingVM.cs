using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models.ViewModels
{
    public class ProcessingVM
    {
        public Processing p { get; set; }
        public string status { get; set; }
        public string ProcDate { get; set; }
    }
}