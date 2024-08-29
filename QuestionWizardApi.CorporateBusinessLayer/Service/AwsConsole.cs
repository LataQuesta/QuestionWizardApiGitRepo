using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class AwsConsole : IAwsConsole
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public AwsConsole()
        {
            this.AccessKey = "AKIAYNH52N4VGKFTABAE";
            this.SecretKey = "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";
        }

        ~AwsConsole()
        {
            Dispose(false);
        }

        public void UploadFileOnAWSS3Bucket(byte[] pdfByte, string bucketName, string SubbucketName, string FileName,string hostname)
        {
            try
            {
               // string accessKey = "AKIAYNH52N4VGKFTABAE";
               // string secretKey = "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";

                string FullbucketPath = bucketName + @"/" + SubbucketName;


                if (hostname.Equals("localhost"))
                {
                    using (var fs = new FileStream(@"E:\Questa\Shared\Data\" + FileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(pdfByte, 0, pdfByte.Length);
                    }
                }
                else
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        memStream.Write(pdfByte, 0, pdfByte.Length);


                        AmazonS3Client s3 = new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), Amazon.RegionEndpoint.APSouth1);

                        using (Amazon.S3.Transfer.TransferUtility tranUtility =
                                     new Amazon.S3.Transfer.TransferUtility(s3))
                        {
                            tranUtility.Upload(memStream, FullbucketPath, FileName);
                            tranUtility.Dispose();
                        }
                        s3.Dispose();
                        memStream.Close();
                    }
                }

                
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public byte[] DownloadFileFromAwsS3Bucket(string bucketName, string SubbucketName,string FileName)
        {
            try
            {
                string FullbucketPath = bucketName + @"/" + SubbucketName;

                AmazonS3Client s3 = new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), Amazon.RegionEndpoint.APSouth1);
                GetObjectRequest getObjectRequest = new GetObjectRequest();
                getObjectRequest.BucketName = FullbucketPath;//"h1modulewisescorecard";
                GetObjectResponse response = new GetObjectResponse();


                getObjectRequest.Key = FileName;//UserModel.UserTestId + "_14_5.png";
                response = s3.GetObject(getObjectRequest);
                MemoryStream memoryStream = new MemoryStream();
                using (Stream responseStream = response.ResponseStream)
                {
                    responseStream.CopyTo(memoryStream);
                }
                byte[] ManagingSelfImgByte = memoryStream.ToArray();
                memoryStream.Close();

                return ManagingSelfImgByte;
            }
            catch(Exception ex)
            {
                throw;
            }
            
        }

        public bool CheckFileExitsOnAwsS3Bucket(string bucketName, string SubbucketName, string FileName)
        {
            try
            {
                string FullbucketPath = bucketName + @"/" + SubbucketName;
                using (AmazonS3Client client = new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), Amazon.RegionEndpoint.APSouth1))
                {
                    S3FileInfo s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, FullbucketPath, FileName);
                    if (s3FileInfo.Exists)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
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

        #endregion
    }
}
