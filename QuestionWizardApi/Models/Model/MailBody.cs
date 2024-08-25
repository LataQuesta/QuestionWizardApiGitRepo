using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionWizardApi.Models
{
    public class MailBody
    {
        public string SenderEmailAddress { get; set; }
        public string Name { get; set; }
        public string RecevierEmailAddress { get; set; }
        public string RecevierFirstName { get; set; }
        public string RecevierLastName { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }
        public string CONFIGSET { get; set; }
        public string HOST { get; set; }
        public string PORT { get; set; }
        public string BODY { get; set; }
        public int ProfileId { get; set; }
        public string BCCEmail { get; set; }
        public string CCEmail { get; set; }
        public bool IsInitialMail { get; set; }
        public bool IsAttachmentSent { get; set; }
    }
}