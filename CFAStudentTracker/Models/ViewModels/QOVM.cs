using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models
{
    public class QOVM
    {
        public long? QueueID { get; set; }
        public IEnumerable<QueuePriority> QOList { get; set; }
    }
}