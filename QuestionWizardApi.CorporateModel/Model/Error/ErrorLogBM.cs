using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    public class ErrorLogBM
    {
        public string Message { get; set; }
        public string ErrorMsg { get; set; }
        public string RequestMethod { get; set; }
        public string RequestUri { get; set; }
        public DateTime TimeUtc { get; set; }
    }


}
