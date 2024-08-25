using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateModel.Model
{
    class ClsEnum
    {
    }

    public enum MstProfile
    {
        FreeAssessment = 1
    }
    
    public enum MainType
    {
        EthicalPerfectionistEnneagramType1 = 1,
        EmpathicNurturerEnneagramType2 = 2,
        AmbitiousAchieverEnneagramType3 = 3,
        IntenseIndividualistEnneagramType4 = 4,
        PerceptiveSpecialistEnneagramType5 = 5,
        DutifulLoyalistEnneagramType6 = 6,
        VersatileVisionaryEnneagramType7 = 7,
        CharismaticControllerEnneagramType8 = 8,
        ReceptivePeacemakerEnneagramType9 = 9
    }

    public enum AssessmentModule
    {
        H1PartAMainType = 1,
        H1PartAMistyping = 2,
        H1PartAAptitude = 3,
        H1PartACompetency = 4,
        MainType = 5,
        Mistyping = 6,
        EnneagramInstincts = 7,
        PersonalityToPresence = 8,
        CenterOfExpression = 9,
        StressAndResilience = 10,
        Competency = 15,
        QTamCompetency = 14,
        QLEAPAndQMAPMainType = 13
    }

    public enum enumType
    {
        MainType = 1,
        FeelingCentre = 2,
        ThinkingCentre = 3,
        ActionCentre = 4
    }

    public enum enumModule
    {
        H1PartA = 1,
        H1PartB = 2,
        QLead = 3,
        QTam = 4,
        StandardReport = 5,
        PremiumReport = 6,
        QLeap = 9,
        QMap =10
    }
}
