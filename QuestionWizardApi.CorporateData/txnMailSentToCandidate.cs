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
    
    public partial class txnMailSentToCandidate
    {
        public int MailSentId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<bool> IsAttachmentReqToCandidate { get; set; }
        public Nullable<bool> IsAttachmentReqToHr { get; set; }
        public string HrName { get; set; }
        public string HrEmailId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmailId { get; set; }
    
        public virtual txnCandidate txnCandidate { get; set; }
    }
}
