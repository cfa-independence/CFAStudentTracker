//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CFAStudentTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WeightedFile
    {
        public short QueueID { get; set; }
        public short FileTypeID { get; set; }
        public decimal WeightedAmount { get; set; }
    
        public virtual FileType FileType { get; set; }
        public virtual Queue Queue { get; set; }
    }
}