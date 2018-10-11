using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models.ViewModels
{
    public class FileDetail
    {
        public List<Record> Rec { get; set; }
        public StudentFile StudFile { get; set; }
    }
}