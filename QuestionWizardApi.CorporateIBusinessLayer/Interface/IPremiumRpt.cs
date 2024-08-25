using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
   public interface IPremiumRpt
    {
        byte[] GeneratePremiumRpt(string RecevierName, int TestId, int MainType, int FellingCenter, int ActionCenter);
        void Dispose();
    }
}
