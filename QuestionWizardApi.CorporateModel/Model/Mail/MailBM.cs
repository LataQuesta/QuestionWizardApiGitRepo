using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    public class MailBM
    {
        public string SenderEmailAddress { get; set; }
        public string Name { get; set; }
        public string RecevierEmailAddress { get; set; }
        public string RecevierName { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }
        public string CONFIGSET { get; set; }
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string BODY { get; set; }
        public string BCCEmail { get; set; }
        public string CCEmail { get; set; }
    }

    public class TempMailBody
    {
        public bool IsSucess { get; set; }
        public ClsMailBodyModel CandidateModel { get;set; }
        public ClsMailBodyModel HrModel { get; set; }
    }

    public class ClsMailBodyModel
    {
        public string SenderEmailAddress { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string RecevierEmailAddress { get; set; } = string.Empty;
        public string RecevierName { get; set; } = string.Empty;
        public string SMTP_USERNAME { get; set; } = string.Empty;
        public string SMTP_PASSWORD { get; set; } = string.Empty;
        public string CONFIGSET { get; set; } = string.Empty;
        public string HOST { get; set; } = string.Empty;
        public string PORT { get; set; } = string.Empty;
        public string BODY { get; set; } = string.Empty;
        public string BCCEmail { get; set; } = string.Empty;
        public string CCEmail { get; set; } = string.Empty;
        public bool SendToHr { get; set; }
        public bool SendToCandidate { get; set; }
    }
}
