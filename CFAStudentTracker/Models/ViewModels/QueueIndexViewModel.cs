using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models
{
    public class QueueIndexViewModel
    {
        public List<Queue> QueueList { get; set; }
        public int numberFiles { get; set; }
        public int numberUsers { get; set; }
    }
}