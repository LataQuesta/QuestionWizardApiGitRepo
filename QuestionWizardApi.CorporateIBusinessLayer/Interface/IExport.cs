using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IExport
    {
        System.IO.MemoryStream GetScoreCard(int TestId);
        System.IO.MemoryStream GetScoreCardDataForQSSER();
        System.IO.MemoryStream GetScoreCardDataForQLeap();
        System.IO.MemoryStream GetScoreCardDataForQLead();
        string GetName(int TestId);
    }
}
