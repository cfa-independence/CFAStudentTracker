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
    
    public partial class Processing
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Processing()
        {
            this.ProcessingError = new HashSet<ProcessingError>();
        }
    
        public long ProcID { get; set; }
        public bool InFilingCabinet { get; set; }
        public System.DateTime ProcInQueue { get; set; }
        public Nullable<System.DateTime> ProcToUser { get; set; }
        public Nullable<System.DateTime> ProcUserComplete { get; set; }
        public string Username { get; set; }
        public short QueueID { get; set; }
        public long RecordID { get; set; }
    
        public virtual Queue Queue { get; set; }
        public virtual Record Record { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProcessingError> ProcessingError { get; set; }
    }
}
