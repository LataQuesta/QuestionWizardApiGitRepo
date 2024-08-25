using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using QuestionWizardApi.Models.Model;
using QuestionWizardApi.Models.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace QuestionWizardApi.Models
{
    public class MailService : Repository<txnCandidate>
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public MailService(QuestionDemoEntities context): base(context) { }
        public QuestionDemoEntities DBEntities
        {
            get
            {
                return db as QuestionDemoEntities;
            }

        }

        public ClsUserModel GetEmailAddress(int TestId)
        {
            try
            {

                ClsUserModel UserModel = db.txnUserTestDetails.Join(db.txnCandidates, x => x.UserId, c => c.UserId, (x, c) => new { x, c })
                                        .Where(x => x.x.TestId == TestId)
                                      .Select(x => new ClsUserModel
                                      {
                                          UserId = x.c.UserId,
                                          Title = x.c.Title,
                                          FirstName = x.c.FirstName,
                                          LastName = x.c.LastName,
                                          PhoneNumber = x.c.PhoneNumber,
                                          UserEmail = x.c.UserEmail,
                                          UserGender = x.c.UserGender.HasValue ? x.c.UserGender.Value : 0,
                                          UserAge = x.c.UserAge.HasValue ? x.c.UserAge.Value : 0,
                                          State = x.c.UserState.HasValue ? x.c.UserState.Value : 0,
                                          Country = x.c.UserCountry.HasValue ? x.c.UserCountry.Value : 0,
                                          Qualification = x.c.UserQualification.HasValue ? x.c.UserQualification.Value : 0,
                                          Professional = x.c.UserProfessional.HasValue ? x.c.UserProfessional.Value : 0,
                                          MaritalStatus = x.c.UserMaritalStatus.HasValue ? x.c.UserMaritalStatus.Value : 0,
                                          EmployeeStatus = x.c.UserEmployeeStatus.HasValue ? x.c.UserEmployeeStatus.Value : 0,
                                          ProfileId = x.c.ProfileSelected.HasValue ? x.c.ProfileSelected.Value : 0,
                                          IsOTPRequire = x.c.IsOTPRequire.HasValue ? x.c.IsOTPRequire.Value : false,
                                          IsActive = x.c.IsActive.HasValue ? x.c.IsActive.Value : false,
                                          IsInitialMail = x.c.IsInitialMail.HasValue ? x.c.IsInitialMail.Value : false,
                                          IsAttachmentSent = x.c.IsAttachmentSent.HasValue ? x.c.IsAttachmentSent.Value : false,
                                      }).FirstOrDefault();

                return UserModel;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public MailBody GetMailConfig(int TestId,int ProfileId,bool IsInitialMail,bool IsFinalMail)
        {
            try
            {
                var MailType = db.mstProfileSelecteds.Where(x => x.ProfileId == ProfileId && x.IsActive == true).FirstOrDefault();
                int MailTypeId = 0;
                if(IsInitialMail && MailType != null)
                {
                    MailTypeId = MailType.InitialMailSent.Value;
                }
                else if(IsFinalMail && MailType != null)
                {
                    MailTypeId = MailType.FinalMail.Value;
                }
                MailBody objMailBody = new MailBody();

                if (MailTypeId > 0)
                {
                    mstMailConfig obj = db.mstMailConfigs.Where(x => x.MailTypeId == MailTypeId).FirstOrDefault();

                    ClsUserModel UserModel = GetEmailAddress(TestId);
                    objMailBody = new MailBody
                    {
                        SenderEmailAddress = obj.FromMailAddress,
                        Name = obj.SMTP_SenderNAME,//"Questa Enneagram Assessment",
                        RecevierEmailAddress = UserModel.UserEmail,
                        RecevierFirstName = UserModel.FirstName,
                        RecevierLastName = UserModel.LastName,
                        SMTP_USERNAME = obj.SMTP_USERNAME,
                        SMTP_PASSWORD = obj.SMTP_PASSWORD,
                        CONFIGSET = obj.CONFIGSET,
                        HOST = obj.HOST,
                        PORT = obj.PORT,
                        BODY = obj.BODY,
                        ProfileId = UserModel.ProfileId,
                        BCCEmail = obj.BCCMailAddress,
                        CCEmail = obj.CCMailAddress,
                        IsInitialMail = UserModel.IsInitialMail,
                        IsAttachmentSent = UserModel.IsAttachmentSent
                    };
                }
               

                return objMailBody;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public void CreateDirectory()
        {
            try
            {
                string root = System.Web.Hosting.HostingEnvironment.MapPath("/Pdf");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void DownloadFileFromS3Bucket(string FileName, string TestId)
        {
            try
            {

                if(!string.IsNullOrEmpty(FileName))
                {
                    string accessKey = ConfigurationManager.AppSettings["S3AccessKey"];
                    string secretKey = ConfigurationManager.AppSettings["S3SecrectKey"];
                    string bucketName = ConfigurationManager.AppSettings["S3BucketName"];

                    TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1));


                    BasicAWSCredentials basicCredentials = new BasicAWSCredentials(accessKey, secretKey);
                    AmazonS3Client s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), Amazon.RegionEndpoint.APSouth1);
                    ListObjectsRequest request = new ListObjectsRequest();

                    ListObjectsResponse response = s3Client.ListObjects(request.BucketName = bucketName, request.Prefix = FileName);
                    string destinationFile = System.Web.Hosting.HostingEnvironment.MapPath("/Pdf");
                    destinationFile = destinationFile + "\\" + TestId;

                    if (!Directory.Exists(destinationFile))
                    {
                        Directory.CreateDirectory(destinationFile);
                    }

                    foreach (S3Object obj in response.S3Objects)
                    {
                        string filename = destinationFile + "\\" + obj.Key;
                        FileStream fs = File.Create(filename);
                        fs.Close();
                        Console.WriteLine("{0}", obj.Key);
                        fileTransferUtility.Download(filename, bucketName, obj.Key);
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        public void SentMail(MailBody ObjMailBody,string TestId,string URL)
        {
            try
            {
                // Replace sender@example.com with your "From" address. 
                // This address must be verified with Amazon SES.
                string FROM = ObjMailBody.SenderEmailAddress;
                string FROMNAME = ObjMailBody.Name;

                // Replace recipient@example.com with a "To" address. If your account 
                // is still in the sandbox, this address must be verified.
                string TO = ObjMailBody.RecevierEmailAddress;
               // string[] BCC = new string[];
                List<string> BCC = new List<string>();
                if (!string.IsNullOrEmpty(ObjMailBody.BCCEmail))
                {
                    BCC = ObjMailBody.BCCEmail.Split(';').ToList();
                }

                List<string> CC = new List<string>();
                if (!string.IsNullOrEmpty(ObjMailBody.CCEmail))
                {
                    CC = ObjMailBody.CCEmail.Split(';').ToList();
                }

                string RecevierName = ObjMailBody.RecevierFirstName; //+ ' ' + ObjMailBody.RecevierLastName;
                // Replace smtp_username with your Amazon SES SMTP user name.
                string SMTP_USERNAME = ObjMailBody.SMTP_USERNAME;

                // Replace smtp_password with your Amazon SES SMTP user name.
                string SMTP_PASSWORD = ObjMailBody.SMTP_PASSWORD;

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                string CONFIGSET = ObjMailBody.CONFIGSET;

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                string HOST = ObjMailBody.HOST;

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = Convert.ToInt32(ObjMailBody.PORT);

                // The subject line of the email
                string SUBJECT =
                    "Questa Enneagram Assessment - Test Login Details";

                // The body of the email
                string HTMLContent = "<html><head><style>.image{margin - left: auto; margin - right: auto;}p{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}";
                HTMLContent = HTMLContent + ".image - container {justify - content: center;}li{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}.border{border: 1px solid black;}</style></head><body>";
                string HTMLEndContent = "</body></html> ";
                string BODY = HTMLContent + ObjMailBody.BODY + HTMLEndContent;
                BODY = BODY.Replace("@RecevierName", RecevierName);
                BODY = BODY.Replace("@URL", URL);
                BODY = BODY.Replace("@Email", TO);
                BODY = BODY.Replace("@TestId", TestId);
                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                //create Alrternative HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(BODY, null, "text/html");

                //Add Image
                string FirstImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaMailLogo.png";
                byte[] FirstImageBytes = System.IO.File.ReadAllBytes(FirstImagePath);
                System.IO.MemoryStream FirstImagestreamBitmap = new System.IO.MemoryStream(FirstImageBytes);
                LinkedResource theEmailImage = new LinkedResource(FirstImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage.ContentId = "myImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage);

                string FooterImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaMailFooterLogo.png";
                byte[] FooterImageBytes = System.IO.File.ReadAllBytes(FooterImagePath);
                System.IO.MemoryStream FooterImagestreamBitmap = new System.IO.MemoryStream(FooterImageBytes);
                LinkedResource theEmailImage1 = new LinkedResource(FooterImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage1.ContentId = "myFooterID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage1);

                string FacebookImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "facebook.png";
                byte[] FacebookImageBytes = System.IO.File.ReadAllBytes(FacebookImagePath);
                System.IO.MemoryStream FacebookImagestreamBitmap = new System.IO.MemoryStream(FacebookImageBytes);
                LinkedResource theEmailImage2 = new LinkedResource(FacebookImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage2.ContentId = "myFacebookID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage2);



                string WWWImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "website.png";
                byte[] WWWImageBytes = System.IO.File.ReadAllBytes(WWWImagePath);
                System.IO.MemoryStream WWWImagestreamBitmap = new System.IO.MemoryStream(WWWImageBytes);
                LinkedResource theEmailImage3 = new LinkedResource(WWWImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage3.ContentId = "myWWWID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage3);



                string ATImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "AtLogo.png";
                byte[] ATImageBytes = System.IO.File.ReadAllBytes(ATImagePath);
                System.IO.MemoryStream ATImagestreamBitmap = new System.IO.MemoryStream(ATImageBytes);
                LinkedResource theEmailImage4 = new LinkedResource(ATImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage4.ContentId = "myAtID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage4);



                string LinkedImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "linkedin.png";
                byte[] LinkedImageBytes = System.IO.File.ReadAllBytes(LinkedImagePath);
                System.IO.MemoryStream LinkedImagestreamBitmap = new System.IO.MemoryStream(LinkedImageBytes);
                LinkedResource theEmailImage5 = new LinkedResource(LinkedImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage5.ContentId = "myLinkedInID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage5);

                //Add view to the Email Message
                message.AlternateViews.Add(htmlView);

                message.From = new MailAddress(FROM, FROMNAME);
                message.To.Add(new MailAddress(TO));
                foreach(var bcc in BCC)
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }
                foreach (var cc in CC)
                {
                    message.CC.Add(new MailAddress(cc));
                }
                message.Subject = SUBJECT;
                message.Body = BODY;
                //string destinationFile = System.Web.Hosting.HostingEnvironment.MapPath("/Pdf");
                //string filename = destinationFile + "\\" + TestId + "." + "pdf";
                //if (File.Exists(filename))
                //{
                //    message.Attachments.Add(new Attachment(filename));
                //}

                // Comment or delete the next line if you are not using a configuration set
                message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                    // Enable SSL encryption
                    client.EnableSsl = true;

                    client.Send(message);
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            
        }
        public void FinalSentMail(MailBody ObjMailBody, string TestId,string FileName)
        {
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            int? TempTestId = Convert.ToInt32(TestId);
            int UserId = DBEntities.txnUserTestDetails.Where(x => x.TestId == TempTestId).Select(x => x.UserId.Value).FirstOrDefault();

            try
            {
                // Replace sender@example.com with your "From" address. 
                // This address must be verified with Amazon SES.
                String FROM = ObjMailBody.SenderEmailAddress;
                String FROMNAME = ObjMailBody.Name;

                // Replace recipient@example.com with a "To" address. If your account 
                // is still in the sandbox, this address must be verified.
                String TO = ObjMailBody.RecevierEmailAddress;
                //  string cc = "divya@questaenneagram.com";
            //    string[] BCC = new string[];
                string[] BCC = ObjMailBody.BCCEmail.Split(';');//"support@questaenneagram.com";

                string RecevierName = ObjMailBody.RecevierFirstName; //+ ' ' + ObjMailBody.RecevierLastName;
                // Replace smtp_username with your Amazon SES SMTP user name.
                String SMTP_USERNAME = ObjMailBody.SMTP_USERNAME;

                // Replace smtp_password with your Amazon SES SMTP user name.
                String SMTP_PASSWORD = ObjMailBody.SMTP_PASSWORD;

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                String CONFIGSET = ObjMailBody.CONFIGSET;

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                String HOST = ObjMailBody.HOST;

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = Convert.ToInt32(ObjMailBody.PORT);

                // The subject line of the email
                String SUBJECT =
                    "Questa Enneagram Assessment - Complete Personality Assessment";

                // The body of the email
                string HTMLContent = "<html><head><style>.image{margin - left: auto; margin - right: auto;}p{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}";
                HTMLContent = HTMLContent + ".image - container {justify - content: center;}li{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}.border{border: 1px solid black;}</style></head><body>";
                string HTMLEndContent = "</body></html> ";
                string BODY = HTMLContent + ObjMailBody.BODY + HTMLEndContent;
                BODY = BODY.Replace("@RecevierName", RecevierName);
               
                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                //create Alrternative HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(BODY, null, "text/html");


                //Add Image
                string HeadingImg = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaMailLogo.png";
                byte[] HeadingImageBytes = System.IO.File.ReadAllBytes(HeadingImg);
                System.IO.MemoryStream HeadingImagestreamBitmap = new System.IO.MemoryStream(HeadingImageBytes);
                LinkedResource theHeadingImg = new LinkedResource(HeadingImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theHeadingImg.ContentId = "myImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theHeadingImg);

                //Add Image
                string FirstImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaLastMailLogo.png";
                byte[] FirstImageBytes = System.IO.File.ReadAllBytes(FirstImagePath);
                System.IO.MemoryStream FirstImagestreamBitmap = new System.IO.MemoryStream(FirstImageBytes);
                LinkedResource LastMailLogo = new LinkedResource(FirstImagestreamBitmap,MediaTypeNames.Image.Jpeg);
                LastMailLogo.ContentId = "myFinalImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(LastMailLogo);


                //Add Image
                string FinalMailFooterImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaLastMailFooterLogo.png";
                byte[] FinalMailFooterImageBytes = System.IO.File.ReadAllBytes(FinalMailFooterImagePath);
                System.IO.MemoryStream FinalMailFooterImagestreamBitmap = new System.IO.MemoryStream(FinalMailFooterImageBytes);
                LinkedResource FinalMailFooterImageLogo = new LinkedResource(FinalMailFooterImagestreamBitmap,MediaTypeNames.Image.Jpeg);
                FinalMailFooterImageLogo.ContentId = "myFinalFooterImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(FinalMailFooterImageLogo);
                
                string FooterImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaMailFooterLogo.png";
                byte[] FooterMailFooterImageBytes = System.IO.File.ReadAllBytes(FooterImagePath);
                System.IO.MemoryStream FooterMailFooterImagestreamBitmap = new System.IO.MemoryStream(FooterMailFooterImageBytes);
                LinkedResource theEmailImage1 = new LinkedResource(FooterMailFooterImagestreamBitmap,MediaTypeNames.Image.Jpeg);
                theEmailImage1.ContentId = "myFooterID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage1);

                string FacebookImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "facebook.png";
                byte[] FacebookMailFooterImageBytes = System.IO.File.ReadAllBytes(FacebookImagePath);
                System.IO.MemoryStream FacebookMailFooterImagestreamBitmap = new System.IO.MemoryStream(FacebookMailFooterImageBytes);
                LinkedResource theEmailImage2 = new LinkedResource(FacebookMailFooterImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage2.ContentId = "myFacebookID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage2);



                string WWWImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "website.png";
                byte[] WWWImageBytes = System.IO.File.ReadAllBytes(WWWImagePath);
                System.IO.MemoryStream WWWImagestreamBitmap = new System.IO.MemoryStream(WWWImageBytes);
                LinkedResource theEmailImage3 = new LinkedResource(WWWImagestreamBitmap,MediaTypeNames.Image.Jpeg);
                theEmailImage3.ContentId = "myWWWID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage3);



                string ATImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "AtLogo.png";
                byte[] ATImageBytes = System.IO.File.ReadAllBytes(ATImagePath);
                System.IO.MemoryStream ATImagestreamBitmap = new System.IO.MemoryStream(ATImageBytes);
                LinkedResource theEmailImage4 = new LinkedResource(ATImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage4.ContentId = "myAtID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage4);



                string LinkedImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "linkedin.png";
                byte[] LinkedImageBytes = System.IO.File.ReadAllBytes(LinkedImagePath);
                System.IO.MemoryStream LinkedImagestreamBitmap = new System.IO.MemoryStream(LinkedImageBytes);
                LinkedResource theEmailImage5 = new LinkedResource(LinkedImagestreamBitmap, MediaTypeNames.Image.Jpeg);
                theEmailImage5.ContentId = "myLinkedInID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage5);

                //Add view to the Email Message
                message.AlternateViews.Add(htmlView);

                //    DownloadFileFromS3Bucket(FileName, TestId);

                message.From = new MailAddress(FROM, FROMNAME);
                message.To.Add(new MailAddress(TO));
                //  message.CC.Add(new MailAddress(cc));
                //  message.CC.Add(new MailAddress("parabavadhut18@gmail.com"));
              
                
              //  message.Bcc.Add(new MailAddress("divya@questaenneagram.com"));
                message.Subject = SUBJECT;
                message.Body = BODY;


                #region S3 Storage Code
                //string accessKey = ConfigurationManager.AppSettings["S3AccessKey"];
                //string secretKey = ConfigurationManager.AppSettings["S3SecrectKey"];
                //string bucketName = ConfigurationManager.AppSettings["S3BucketName"];

                ////  MailMessage mail = new MailMessage();
                ////Create a MemoryStream from a file for this test
                //var s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
                //GetObjectRequest request = new GetObjectRequest();
                //request.BucketName = bucketName;
                //request.Key = FileName;
                //// GetObjectResponse response = s3Client.GetObject(request);
                ////  response.WriteResponseStreamToFile(@"C:\Desktop\Sample.txt");
                //Byte[] bytes;
                //GetObjectResponse response = s3Client.GetObject(request);
                //using (Stream responseStream = response.ResponseStream)
                //{
                //    bytes = ReadStream(responseStream);
                //}
                #endregion


                if (ObjMailBody.IsAttachmentSent)
                {
                    foreach (var bcc in BCC)
                    {
                        message.Bcc.Add(new MailAddress(bcc));
                    }

                    byte[] pdfResult = PdfGenerate(RecevierName, FileName);

                    var memStream = new MemoryStream(pdfResult);
                    memStream.Position = 0;
                    var contentType = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);
                    var reportAttachment = new Attachment(memStream, contentType);

                    string Profile = ObjMailBody.ProfileId == 1 ? "Standard" : 
                                     ObjMailBody.ProfileId == 2 ? "Premium" : 
                                     ObjMailBody.ProfileId == 3 ? "Premium Plus" : 
                                     ObjMailBody.ProfileId >= 4 ? "Free Assessment" : "";


                    reportAttachment.ContentDisposition.FileName = RecevierName + "-Questa Enneagram Assessment Profile(" + Profile + ")" + ".pdf";
                    message.Attachments.Add(reportAttachment);
                }
                else if (!ObjMailBody.IsAttachmentSent)
                {
                    SentFinalEmailToSupport(ObjMailBody, TestId, FileName);
                }
               
                // Comment or delete the next line if you are not using a configuration set
                message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                    // Enable SSL encryption
                    client.EnableSsl = true;

                    client.Send(message);

                    client.Dispose();
                    
                }

                message.Dispose();
                //  DeleteDirectoryAndFile(TestId);
                if (ObjMailBody.IsAttachmentSent)
                {
                    SaveAttachmentDetails(TempTestId.Value, UserId, FileName, true, DateTime);
                }
            }
            catch(Exception ex)
            {
               // SaveAttachmentDetails(TempTestId.Value, UserId, FileName, false, DateTime);
                throw;
            }
           
        }


        public  byte[] PdfGenerate(string RecevierName,string FileName)
        {
            try
            {
                byte[] pdfResult = new byte[16 * 1024];

                string imageURL = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "Cover Page_InitialPdf.png";

                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

                jpg.ScaleToFit(4040f, 700f);

                jpg.SetAbsolutePosition(60, 0);

                jpg.Alignment = iTextSharp.text.Image.UNDERLYING;


                PdfPTable FinalTable = new PdfPTable(2);
                FinalTable.HorizontalAlignment = 0;//0=Left, 1=Centre, 2=Right
                FinalTable.TotalWidth = 870;
                FinalTable.LockedWidth = true;
                FinalTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


                Phrase paraPhrase = new Phrase();
                var nameChunkNormalFont = FontFactory.GetFont("Crimson Text", 20);
                Chunk nameChunk = new Chunk("Self - Discovery Profile \n", nameChunkNormalFont);
                paraPhrase.Add(new Paragraph(nameChunk));
                paraPhrase.Add(new Paragraph("\n"));
                var doctorType1ChunkNormalFont = FontFactory.GetFont("Crimson Text", 14);
                Chunk doctorType1Chunk = new Chunk(RecevierName + "\n", doctorType1ChunkNormalFont);
                paraPhrase.Add(new Paragraph(doctorType1Chunk));
                paraPhrase.Add(new Paragraph("\n"));
                var doctorType2ChunkNormalFont = FontFactory.GetFont("Crimson Text", 14);
                Chunk doctorType2Chunk = new Chunk(DateTime.Now.ToString("dddd, dd MMMM yyyy") + "\n", doctorType1ChunkNormalFont);
                paraPhrase.Add(new Paragraph(doctorType2Chunk));

                paraPhrase.Add(new Paragraph("\n"));

                PdfPCell Column2 = new PdfPCell(paraPhrase);
                Column2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                FinalTable.AddCell(Column2);


                string LogImage = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaLog_InitialPdf.png";
                iTextSharp.text.Image LogoImage = iTextSharp.text.Image.GetInstance(LogImage);
                LogoImage.ScaleToFit(70f, 70f);
                PdfPCell Column1 = new PdfPCell(LogoImage);

                Column1.Border = iTextSharp.text.Rectangle.NO_BORDER;
                FinalTable.AddCell(Column1);


                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(3.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));


                byte[] htmlResult;

                using (MemoryStream htmlStream = new MemoryStream())
                {
                    Document htmlDoc = new Document();
                    PdfWriter htmlWriter = PdfWriter.GetInstance(htmlDoc, htmlStream);
                    htmlDoc.Open();

                    HTMLWorker htmlWorker = new HTMLWorker(htmlDoc);
                    htmlDoc.Add(FinalTable);
                    htmlDoc.Add(p);
                    // htmlWorker.Parse(htmlStringReader);
                    htmlDoc.Add(jpg);
                    htmlDoc.Close();
                    htmlResult = htmlStream.ToArray();
                }

                

                using (MemoryStream pdfStream = new MemoryStream())
                {
                    Document doc1 = new Document();
                    PdfCopy copyWriter = new PdfCopy(doc1, pdfStream);
                    doc1.Open();

                    PdfReader htmlPdfReader = new PdfReader(htmlResult);
                    AppendPdf(copyWriter, htmlPdfReader); // your foreach pdf code here
                    htmlPdfReader.Close();
                    string AttachmentFilePath = System.Web.Hosting.HostingEnvironment.MapPath("/Pdf") + "\\" + FileName;
                    PdfReader attachmentReader = new PdfReader(AttachmentFilePath);
                    AppendPdf(copyWriter, attachmentReader);
                    attachmentReader.Close();

                    doc1.Close();

                    pdfResult = pdfStream.ToArray();
                }

                return pdfResult;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void AppendPdf(PdfCopy writer, PdfReader reader)
        {
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                PdfImportedPage page = writer.GetImportedPage(reader, i);
                writer.AddPage(page);
            }
        }

        public static byte[] ReadStream(Stream responseStream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public void SaveAttachmentDetails(int TestId,int UserId,string FileName,bool flg,DateTime dt)
        {
            try
            {
                if (!DBEntities.txnAttachmentMailSents.Any(x => x.TestId == TestId))
                {
                    txnAttachmentMailSent objAttachmentMail = new txnAttachmentMailSent();
                    objAttachmentMail.TestId = TestId;
                    objAttachmentMail.FileName = FileName;
                    objAttachmentMail.MailSentFlg = flg;
                    objAttachmentMail.CreatedAt = dt;
                    objAttachmentMail.CreatedBy = UserId;
                    objAttachmentMail.LastModifiedAt = dt;
                    objAttachmentMail.LastModifiedBy = UserId;
                    DBEntities.txnAttachmentMailSents.Add(objAttachmentMail);
                    DBEntities.SaveChanges();
                }
                else
                {
                    txnAttachmentMailSent objAttMailSent = DBEntities.txnAttachmentMailSents.Where(x => x.TestId == TestId).FirstOrDefault();
                    objAttMailSent.MailSentFlg = flg;
                    objAttMailSent.CreatedAt = dt;
                    objAttMailSent.CreatedBy = UserId;
                    objAttMailSent.LastModifiedAt = dt;
                    objAttMailSent.LastModifiedBy = UserId;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void DeleteDirectoryAndFile(string TestId)
        {
            try
            {
                string destinationFile = System.Web.Hosting.HostingEnvironment.MapPath("/Pdf");
                destinationFile = destinationFile + "\\" + TestId;
               // destinationFile = destinationFile + "//" + FileName + "." + "pdf";
                System.IO.DirectoryInfo di = new DirectoryInfo(destinationFile);

               
                foreach (FileInfo file in di.GetFiles())
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }
                if(Directory.Exists(destinationFile))
                {
                    Directory.Delete(destinationFile);
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        public void SentFinalEmailToSupport(MailBody ObjMailBody, string TestId, string FileName)
        {
            DateTime DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            int? TempTestId = Convert.ToInt32(TestId);
            int UserId = DBEntities.txnUserTestDetails.Where(x => x.TestId == TempTestId).Select(x => x.UserId.Value).FirstOrDefault();

            try
            {
                // Replace sender@example.com with your "From" address. 
                // This address must be verified with Amazon SES.
                String FROM = ObjMailBody.SenderEmailAddress;
                String FROMNAME = ObjMailBody.Name;

                // Replace recipient@example.com with a "To" address. If your account 
                // is still in the sandbox, this address must be verified.
                String TO = ObjMailBody.RecevierEmailAddress;
                //  string cc = "divya@questaenneagram.com";
                string[] BCC = ObjMailBody.BCCEmail.Split(';');//"support@questaenneagram.com";

                string RecevierName = ObjMailBody.RecevierFirstName; //+ ' ' + ObjMailBody.RecevierLastName;
                // Replace smtp_username with your Amazon SES SMTP user name.
                String SMTP_USERNAME = ObjMailBody.SMTP_USERNAME;

                // Replace smtp_password with your Amazon SES SMTP user name.
                String SMTP_PASSWORD = ObjMailBody.SMTP_PASSWORD;

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                String CONFIGSET = ObjMailBody.CONFIGSET;

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                String HOST = ObjMailBody.HOST;

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = Convert.ToInt32(ObjMailBody.PORT);

                // The subject line of the email
                String SUBJECT =
                    "Questa Enneagram Assessment - Complete Personality Assessment";

                // The body of the email
                string HTMLContent = "<html><head><style>.image{margin - left: auto; margin - right: auto;}p{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}";
                HTMLContent = HTMLContent + ".image - container {justify - content: center;}li{font-size: 12px;font-family: Arial, Helvetica, sans-serif;text-align: justify;text-align-last: left;-moz-text-align-last: left;}.border{border: 1px solid black;}</style></head><body>";
                string HTMLEndContent = "</body></html> ";
                string BODY = HTMLContent + ObjMailBody.BODY + HTMLEndContent;
                BODY = BODY.Replace("@RecevierName", RecevierName);

                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                //create Alrternative HTML view
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(BODY, null, "text/html");

                //Add Image
                string FirstImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaLastMailLogo.png";
                LinkedResource LastMailLogo = new LinkedResource(FirstImagePath);
                LastMailLogo.ContentId = "myFinalImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(LastMailLogo);


                //Add Image
                string FinalMailFooterImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaLastMailFooterLogo.png";
                LinkedResource FinalMailFooterImageLogo = new LinkedResource(FinalMailFooterImagePath);
                FinalMailFooterImageLogo.ContentId = "myFinalFooterImageID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(FinalMailFooterImageLogo);

                string FooterImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "QuestaMailFooterLogo.png";
                LinkedResource theEmailImage1 = new LinkedResource(FooterImagePath);
                theEmailImage1.ContentId = "myFooterID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage1);

                string FacebookImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "facebook.png";
                LinkedResource theEmailImage2 = new LinkedResource(FacebookImagePath);
                theEmailImage2.ContentId = "myFacebookID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage2);



                string WWWImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "website.png";
                LinkedResource theEmailImage3 = new LinkedResource(WWWImagePath);
                theEmailImage3.ContentId = "myWWWID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage3);



                string ATImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "AtLogo.png";
                LinkedResource theEmailImage4 = new LinkedResource(ATImagePath);
                theEmailImage4.ContentId = "myAtID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage4);



                string LinkedImagePath = System.Web.Hosting.HostingEnvironment.MapPath("/Images") + "\\" + "linkedin.png";
                LinkedResource theEmailImage5 = new LinkedResource(LinkedImagePath);
                theEmailImage5.ContentId = "myLinkedInID";

                //Add the Image to the Alternate view
                htmlView.LinkedResources.Add(theEmailImage5);

                //Add view to the Email Message
                message.AlternateViews.Add(htmlView);

                message.From = new MailAddress(FROM, FROMNAME);

                foreach (var bcc in BCC)
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }
                
                message.Subject = SUBJECT;
                message.Body = BODY;


                byte[] pdfResult = PdfGenerate(RecevierName, FileName);

                var memStream = new MemoryStream(pdfResult);
                memStream.Position = 0;
                var contentType = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);
                var reportAttachment = new Attachment(memStream, contentType);

                string Profile = "Free Assessment";


                reportAttachment.ContentDisposition.FileName = RecevierName + "-Questa Enneagram Assessment Profile(" + Profile + ")" + ".pdf";
                message.Attachments.Add(reportAttachment);

                // Comment or delete the next line if you are not using a configuration set
                message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                    // Enable SSL encryption
                    client.EnableSsl = true;

                    client.Send(message);

                    client.Dispose();

                }

                message.Dispose();

                SaveAttachmentDetails(TempTestId.Value, UserId, FileName, true, DateTime);
            }
            catch (Exception ex)
            {
                // SaveAttachmentDetails(TempTestId.Value, UserId, FileName, false, DateTime);
                throw;
            }

        }


    }
}