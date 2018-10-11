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
    
    public partial class Record
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Record()
        {
            this.Note = new HashSet<Note>();
            this.Processing = new HashSet<Processing>();
        }
    
        public long RecordID { get; set; }
        public bool ProcPriority { get; set; }
        public Nullable<System.DateTime> DOD { get; set; }
        public Nullable<System.DateTime> LDA { get; set; }
        public short FileTypeID { get; set; }
        public int FileID { get; set; }
        public string DependencyStatus { get; set; }
        public Nullable<byte> AcademicYear { get; set; }
        public Nullable<decimal> SubAggLimit { get; set; }
        public Nullable<decimal> CombinedAggLimit { get; set; }
        public bool IsProratedLoan { get; set; }
        public Nullable<byte> NumCredits { get; set; }
        public Nullable<bool> ExistingAYEndsBeforeTermTwo { get; set; }
        public Nullable<decimal> SubAmountUsed { get; set; }
        public Nullable<decimal> UnsubAmountUsed { get; set; }
        public Nullable<byte> SumUsagePeriods { get; set; }
        public string AttendanceTermOne { get; set; }
        public string AttendanceTermTwo { get; set; }
        public Nullable<byte> NumAcademicYearsInProgram { get; set; }
        public string AwardYear { get; set; }
        public Nullable<int> EFC { get; set; }
        public Nullable<decimal> LEU { get; set; }
        public Nullable<decimal> PercentPellUsed { get; set; }
        public string StatusTermOne { get; set; }
        public string StatusTermTwo { get; set; }
        public string StatusTermThree { get; set; }
        public bool IsOnlineStudent { get; set; }
        public string BudgetAwardYear { get; set; }
        public string StateOnISIR { get; set; }
        public bool IsWithParents { get; set; }
        public Nullable<decimal> CostPerCredit { get; set; }
        public Nullable<byte> NumMonthsInAY { get; set; }
        public Nullable<byte> NumEstimatedCredits { get; set; }
    
        public virtual FileType FileType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Note { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Processing> Processing { get; set; }
        public virtual StudentFile StudentFile { get; set; }
    }
}