using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IError
    {
        void SaveErrorLog(ErrorLogBM ErrorLog);
        void Dispose();
    }
}
