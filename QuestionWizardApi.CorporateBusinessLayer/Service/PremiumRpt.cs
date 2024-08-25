using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.html.simpleparser;
using System.Net.Mail;
using System.Net;
using iTextSharp.text.pdf.parser;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class PremiumRpt : IPremiumRpt, IDisposable
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities();

        ~PremiumRpt()
        {
            Dispose(false);
        }

        public byte[] GeneratePremiumRpt(string RecevierName, int TestId, int MainType, int FellingCenter, int ActionCenter)
        {


            var buildDir = AppDomain.CurrentDomain.BaseDirectory;

            string imageURL = buildDir + "\\Images\\" + "Cover Page_InitialPdf.png";
            iTextSharp.text.Image CoverImg = iTextSharp.text.Image.GetInstance(imageURL);
           CoverImg.ScaleToFit(4570, 750);
           // CoverImg.ScaleToFit(3000,500);
            CoverImg.SetAbsolutePosition(40, 0);
            CoverImg.Alignment = iTextSharp.text.Image.UNDERLYING;
         //   CoverImg.ScalePercent(20f);


            PdfPTable FinalTable = new PdfPTable(2);
            FinalTable.HorizontalAlignment = 0;//0=Left, 1=Centre, 2=Right
                                               // var colWidthPercentages = new[] { 6f, 10f, 10f, 10f, 20f, 20f, 8f, 8f, 8f };
                                               // FinalTable.SetWidths(colWidthPercentages);
            FinalTable.TotalWidth = 870;
            FinalTable.LockedWidth = true;
            FinalTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


            Phrase paraPhrase = new Phrase();

            iTextSharp.text.Font myFont = FontFactory.GetFont("Crimson Tex", 27, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            paraPhrase.Add(new Paragraph("Your Enneagram Profile \n", myFont));
            paraPhrase.Add(new Paragraph("\n"));

            iTextSharp.text.Font myFont1 = FontFactory.GetFont("Crimson Tex", 20, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            paraPhrase.Add(new Paragraph(RecevierName + "\n", myFont1));
            paraPhrase.Add(new Paragraph("\n"));

            iTextSharp.text.Font myFont2 = FontFactory.GetFont("Crimson Tex", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            paraPhrase.Add(new Paragraph(DateTime.Now.ToString("dddd, dd MMMM yyyy") + "\n", myFont2));
            paraPhrase.Add(new Paragraph("\n"));

            PdfPCell NameColumn = new PdfPCell(paraPhrase);
            NameColumn.Border = iTextSharp.text.Rectangle.NO_BORDER;
            // NameColumn.Width = 20f;
            FinalTable.AddCell(NameColumn);


            string LogImage = buildDir + "\\Images\\" + "QuestaLog_InitialPdf.png";
            iTextSharp.text.Image LogoImage = iTextSharp.text.Image.GetInstance(LogImage);
            LogoImage.ScaleToFit(70f, 70f);
            LogoImage.SetAbsolutePosition(30, 10);
            PdfPCell LogoColumn = new PdfPCell(LogoImage);
            LogoColumn.Border = iTextSharp.text.Rectangle.NO_BORDER;
            FinalTable.AddCell(LogoColumn);


            //PdfPCell Col3 = new PdfPCell(CoverImg,true);
            //Col3.Colspan = 2;
            //Col3.Border = iTextSharp.text.Rectangle.NO_BORDER;
            //FinalTable.AddCell(Col3);

            Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(3.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));




          

            byte[] htmlResult = CreateDynamicPage(FinalTable, CoverImg, p);




            string MainTypeFileName = "Type_" + MainType + "_Main.pdf";
            string MainTypeFilePath = buildDir + "\\Pdf\\PremiumReport\\MainType\\" + MainTypeFileName;


            byte[] pdfResult = GetStaticPDFAndAppend(MainTypeFilePath, htmlResult);




            MailMessage mm = new MailMessage("assessment@questa.in", "parabavadhut18@gmail.com");
            mm.Subject = "iTextSharp PDF";
            mm.Body = "iTextSharp PDF Attachment";
            mm.Attachments.Add(new Attachment(new MemoryStream(pdfResult), "iTextSharpPDF.pdf"));
            mm.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "email-smtp.ap-south-1.amazonaws.com";
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = "AKIAYNH52N4VNGNPG774";
            NetworkCred.Password = "BLhNdOkLRjvzh2VQO3v5dgu5AK1LnBpYFVUWQljxEP5e";
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            smtp.Send(mm);

            return pdfResult;
        }

        public void AppendPdf(PdfCopy writer, PdfReader reader)
        {
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                PdfImportedPage page = writer.GetImportedPage(reader, i);
                writer.AddPage(page);
            }
        }

        public byte[] CreateDynamicPage(PdfPTable HTMLTbl, iTextSharp.text.Image Image, Paragraph p)
        {
            try
            {
                Byte[] HtmlResult;
                using (MemoryStream htmlStream = new MemoryStream())
                {
                    Document htmlDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter htmlWriter = PdfWriter.GetInstance(htmlDoc, htmlStream);
                    htmlDoc.Open();
                    htmlDoc.Add(HTMLTbl);
                    htmlDoc.Add(p);
                    htmlDoc.Add(Image);
                    htmlWriter.PageEvent = new HeaderFooter();
                    htmlDoc.Close();
                    HtmlResult = htmlStream.ToArray();
                }
                return HtmlResult;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public byte[] GetStaticPDFAndAppend(string FilePath,byte[] HTMLResult)
        {
            try
            {
                byte[] pdfResult;

                using (MemoryStream pdfStream = new MemoryStream())
                {
                    Document doc1 = new Document(PageSize.A4, 10f, 10f, 10f, 0f);


                    PdfCopy copyWriter = new PdfCopy(doc1, pdfStream);
                    doc1.Open();

                    PdfReader htmlPdfReader = new PdfReader(HTMLResult);
                    AppendPdf(copyWriter, htmlPdfReader); // your foreach pdf code here
                    htmlPdfReader.Close();
                    
                    PdfReader attachmentReader = new PdfReader(FilePath);
                    AppendPdf(copyWriter, attachmentReader);
                    attachmentReader.Close();
                    //  PdfWriter pdfWriter = PdfWriter.GetInstance(doc1, pdfStream);
                    //  pdfWriter.PageEvent = new HeaderFooter();
                    doc1.Close();

                    pdfResult = pdfStream.ToArray();
                }

                return pdfResult;
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
        #endregion
    }

    


    class HeaderFooter : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfPTable tbFooter = new PdfPTable(2);
            tbFooter.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

            tbFooter.DefaultCell.Border = 0;
            //  tbFooter.AddCell(new Paragraph());
            Phrase paraPhrase = new Phrase();
            var buildDir = AppDomain.CurrentDomain.BaseDirectory;

            string LogImage = buildDir + "\\Images\\" + "QE Trans_small.png";
            iTextSharp.text.Image LogoImage = iTextSharp.text.Image.GetInstance(LogImage);
            // LogoImage.ScaleToFit(20f, 20f);
            Chunk nameChunk1 = new Chunk(LogoImage, -4f, -4f);
            paraPhrase.Add(new Paragraph(nameChunk1));


            var nameChunkNormalFont = FontFactory.GetFont("Crimson Text", 8);
            Chunk nameChunk = new Chunk("Copyright (2020) Questa Enneagram", nameChunkNormalFont);
            paraPhrase.Add(new Paragraph(nameChunk));



            PdfPCell _cell = new PdfPCell(paraPhrase);
            _cell.Border = PdfPCell.TOP_BORDER;
            _cell.HorizontalAlignment = Element.ALIGN_LEFT;
            // _cell.Border = 0;
            _cell.PaddingTop = 10f;
            _cell.PaddingLeft = 20f;
            tbFooter.AddCell(_cell);



            Phrase paraPhrase1 = new Phrase();
            var nameChunkNormalFont1 = FontFactory.GetFont("Crimson Text", 8);
            Chunk nameChunk2 = new Chunk("Page" + writer.PageNumber, nameChunkNormalFont1);
            paraPhrase1.Add(new Paragraph(nameChunk2));

            _cell = new PdfPCell(paraPhrase1);
            _cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            _cell.Border = PdfPCell.TOP_BORDER;
            _cell.PaddingTop = 10f;

            tbFooter.AddCell(_cell);
            //   tbFooter.WriteSelectedRows(0, -1, 415, 30, writer.DirectContent);
            tbFooter.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetBottom(document.Bottom) - 5, writer.DirectContent);


        }
    }
}
