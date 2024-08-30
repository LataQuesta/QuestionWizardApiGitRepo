SelectPdf Online API - Client Library for .NET
==============================================

HTML to PDF REST API – SelectPdf HTML To PDF Online REST API is a professional solution that lets you create PDF from web pages and raw HTML code in your applications.

PDF to TEXT REST API – SelectPdf Pdf To Text REST API is an online solution that lets you extract text from your PDF documents or search your PDF document for certain words.

PDF Merge REST API – SelectPdf Pdf Merge REST API is an online solution that lets you merge local or remote PDFs into a final PDF document.


Online documentation: https://selectpdf.com/online-api/docs/html/welcome.htm
Client library source code: https://github.com/selectpdf/selectpdf-api-dotnet-client
Samples C#: https://github.com/selectpdf/selectpdf-api-dotnet-client/tree/master/samples/csharp
Samples VB.NET: https://github.com/selectpdf/selectpdf-api-dotnet-client/tree/master/samples/vbnet


Code Sample
===========

using System;
using SelectPdf.Api;

namespace SelectPdf.Api.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            String url = "https://selectpdf.com";
            String outFile = "test.pdf";

            try
            {
                HtmlToPdfClient api = new HtmlToPdfClient("Your key here");
                api
                    .setPageSize(PageSize.A4)
                    .setPageOrientation(PageOrientation.Portrait)
                    .setMargins(0)
                    .setNavigationTimeout(30)
                    .setShowPageNumbers(false)
                    .setPageBreaksEnhancedAlgorithm(true)
                ;

                Console.WriteLine("Starting conversion ...");

                api.convertUrlToFile(url, outFile);

                Console.WriteLine("Conversion finished successfully!");


                UsageClient usage = new UsageClient("Your key here");
                UsageInformation info = usage.getUsage(false);
                Console.WriteLine("Conversions left this month: " + info.Available);

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
