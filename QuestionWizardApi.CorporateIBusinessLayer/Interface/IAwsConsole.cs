

namespace QuestionWizardApi.CorporateIBusinessLayer.Interface
{
    public interface IAwsConsole
    {
        void UploadFileOnAWSS3Bucket(byte[] pdfByte, string bucketName, string SubbucketName, string FileName, string hostname);
        byte[] DownloadFileFromAwsS3Bucket(string bucketName, string SubbucketName, string FileName);
        bool CheckFileExitsOnAwsS3Bucket(string bucketName, string SubbucketName, string FileName);
        void Dispose();
    }
}
