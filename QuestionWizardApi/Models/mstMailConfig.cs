//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuestionWizardApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class mstMailConfig
    {
        public int ConfigId { get; set; }
        public string SMTP_SenderNAME { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }
        public string CONFIGSET { get; set; }
        public string FromMailAddress { get; set; }
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string BODY { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> MailTypeId { get; set; }
        public string BCCMailAddress { get; set; }
        public string CCMailAddress { get; set; }
    }
}
