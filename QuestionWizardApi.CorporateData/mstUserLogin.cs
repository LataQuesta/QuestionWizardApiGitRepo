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
    
    public partial class mstUserLogin
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedAt { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public string RefreshToken { get; set; }
        public Nullable<System.DateTime> RefreshTokenExpiryTime { get; set; }
    }
}
