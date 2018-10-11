using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFAStudentTracker.Models
{
    public class StringAndInt
    {
        public string MyString { get; set; }
        public double MyInt { get; set; }
    }

    public class SSAndInt : StringAndInt
    {
        public string secondString { get; set; }
    }
}