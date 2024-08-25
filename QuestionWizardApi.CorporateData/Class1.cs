using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateData
{
    public class Class1
    {
    }
    public class MasterDataModel
    {
        public List<mstCountry> Countries { get; set; }
        public List<mstQualification> Qualification { get; set; }
        public List<mstProfession> Profession { get; set; }
        public List<mstAge> Ages { get; set; }
        public List<mstGender> Gender { get; set; }
        public List<mstMaritalStatu> MaritalStatus { get; set; }
        public List<mstEmployeeStatu> EmployeeStatus { get; set; }
        public List<mstIndustry> Industry { get; set; }
        public List<mstExperence> Experience { get; set; }
    }
}
