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
    
    public partial class mstIndustry
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mstIndustry()
        {
            this.txnIndustries = new HashSet<txnIndustry>();
        }
    
        public int IndustryId { get; set; }
        public string IndustryName { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<txnIndustry> txnIndustries { get; set; }
    }
}
