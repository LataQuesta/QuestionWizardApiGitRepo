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
    
    public partial class sp_getmailbodyforcandidate_Result
    {
        public string SenderEmailAddress { get; set; }
        public string SenderName { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }
        public string CONFIGSET { get; set; }
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string BODY { get; set; }
        public string BCCEmail { get; set; }
        public string CCEmail { get; set; }
        public Nullable<bool> SendToHr { get; set; }
        public Nullable<bool> SendToCandidate { get; set; }
        public string RecevierName { get; set; }
        public string RecevierEmailAddress { get; set; }
    }
}
