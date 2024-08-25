using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class ReportGeneration : IReportGeneration
    {
        MailServiceForH1 _mailservice;
        MailService _mailservice_1;
        AwsConsole _awsconsole;
        QuestionService _questionservice;
        public string BucketNameForPdf { get; set; }
        public ReportGeneration()
        {
            _mailservice = new MailServiceForH1();
            _awsconsole = new AwsConsole();
            _questionservice = new QuestionService();
            _mailservice_1 = new MailService();

            this.BucketNameForPdf = "questareportpdfformat";
        }

        ~ReportGeneration()
        {
            Dispose(false);
        }

        public async Task GenerateCandidateReport(int TestId,int AssessmentId,string CandidateName, string hostname)
        {
            try
            {
                string FileName = TestId + "-Questa Enneagram Assessment Profile.pdf";
                switch (AssessmentId)
                {
                    case (int)enumModule.H1PartA:

                        byte[] QsserByte = _mailservice.GenerateQsssrReport(TestId, hostname);
                        _awsconsole.UploadFileOnAWSS3Bucket(QsserByte, BucketNameForPdf, "Qsser", FileName, hostname);

                        break;
                    case (int)enumModule.StandardReport:

                        var TypeId = _questionservice.GetFileNameForStandardReport(Convert.ToInt32(TestId));
                        byte[] StandardByte = _mailservice_1.PdfGenerate(CandidateName, TypeId.Item2, TypeId.Item1);
                        _awsconsole.UploadFileOnAWSS3Bucket(StandardByte, BucketNameForPdf, "Standard", FileName, hostname);

                        break;
                    case (int)enumModule.QMap:

                        byte[] QmapByte = _mailservice.GenerateQMapReport(Convert.ToInt32(TestId), hostname);
                        _awsconsole.UploadFileOnAWSS3Bucket(QmapByte, BucketNameForPdf, "Qmap", FileName, hostname);

                        break;
                    case (int)enumModule.QLeap:

                        byte[] QleapByte = _mailservice.GenerateQLeapReport(Convert.ToInt32(TestId), hostname);
                        _awsconsole.UploadFileOnAWSS3Bucket(QleapByte, BucketNameForPdf, "Qleap", FileName, hostname);

                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }






        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Console.WriteLine("This is the first call to Dispose. Necessary clean-up will be done!");

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Console.WriteLine("Explicit call: Dispose is called by the user.");
                }
                else
                {
                    // Console.WriteLine("Implicit call: Dispose is called through finalization.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Console.WriteLine("Unmanaged resources are cleaned up here.");

                // TODO: set large fields to null.

                disposedValue = true;
            }
            else
            {
                // Console.WriteLine("Dispose is called more than one time. No need to clean up!");
            }
        }



        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        public List<ClsSet6ScoreModel> lstSubModuleScore(int Testid, int setid)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
