//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuestionWizardApi.CorporateData
{
    using System;
    using System.Collections.Generic;
    
    public partial class mstAssessmentSet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mstAssessmentSet()
        {
            this.txnDynamicMisTypings = new HashSet<txnDynamicMisTyping>();
            this.txnModuleByAssessments = new HashSet<txnModuleByAssessment>();
            this.mstAssessmentModules = new HashSet<mstAssessmentModule>();
            this.mst_mailConfigByAssessment = new HashSet<mst_mailConfigByAssessment>();
            this.mstmailConfigByAssessments = new HashSet<mstmailConfigByAssessment>();
            this.txnCampanyMapToAssessments = new HashSet<txnCampanyMapToAssessment>();
            this.txnHrMapToCompanyAndAssessments = new HashSet<txnHrMapToCompanyAndAssessment>();
            this.TxnLinkGenerationIds = new HashSet<TxnLinkGenerationId>();
        }
    
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public string CreateBy { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public Nullable<System.DateTime> LastModifiedAt { get; set; }
        public Nullable<int> TotalQuestion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<txnDynamicMisTyping> txnDynamicMisTypings { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<txnModuleByAssessment> txnModuleByAssessments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mstAssessmentModule> mstAssessmentModules { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mst_mailConfigByAssessment> mst_mailConfigByAssessment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mstmailConfigByAssessment> mstmailConfigByAssessments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<txnCampanyMapToAssessment> txnCampanyMapToAssessments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<txnHrMapToCompanyAndAssessment> txnHrMapToCompanyAndAssessments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TxnLinkGenerationId> TxnLinkGenerationIds { get; set; }
    }
}
