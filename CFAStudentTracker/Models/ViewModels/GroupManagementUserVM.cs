using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models.ViewModels
{
    public class GroupManagementUserVM
    {
        public string User { get; set; }
        public double ProcessedToday { get; set; }
        public double OutstandingErrors { get; set; }
    }
}