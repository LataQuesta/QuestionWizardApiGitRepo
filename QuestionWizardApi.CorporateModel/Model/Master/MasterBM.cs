using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    public class MasterBM
    {
        public List<AgeBM> Ages { get; set; }
        public List<CountryBM> Countries { get; set; }
        public List<QualificationBM> Qualification { get; set; }
        public List<EmployeeStatusBM> EmployeeStatus { get; set; }
        public List<GenderBM> Gender { get; set; }
        public List<IndustryBM> Industry { get; set; }
        public List<MaritalStatusBM> MaritalStatus { get; set; }
        public List<ProfessionBM> Profession { get; set; }
        public List<ExperienceBM> Experience { get; set; }
    }
}
