using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestionWizardApi.CorporateModel.Model;

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IMasterData
    {
        Task<MasterBM> GetMasterData();
        Task<List<StateBM>> GetState(int CountryId);
        void Dispose();
    }
}
