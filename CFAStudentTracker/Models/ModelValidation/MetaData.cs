using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CFAStudentTracker.Models
{
    public class StudentFileAnnotation
    {
        [Required]
        [RegularExpression("^\\d{3}-\\d{2}-\\d{4}$", ErrorMessage = "Invalid SSN Format; xxx-xx-xxxx")]
        public string FileSSN { get; set; }
        public string FileName { get; set; }
    }
}