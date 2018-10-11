using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models.ViewModels
{
    public class LookUp
    {
        public Processing p { get; set; }
        public Record r { get; set; }
        public StudentFile f { get; set; }
        public FileType t { get; set; }
        public ProcessingError e { get; set; }
        public ErrorType et { get; set; }
        public ErrorComplete c { get; set; }
        
    }
}