using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Lambda.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RosterApiLambda.Helpers
{
    public class ReportHelper
    {
        public static string[] speciesCc = new[] { "CC" };
        public static string[] speciesCm = new[] { "CM" };
        public static string[] speciesDc = new[] { "DC" };
        public static string[] speciesOther = new[] { "LK", "LO", "EI", "HB" };
        public static string[] speciesUnknown = new[] { "XX", "", null };

        public static void Merge(List<string> fromFileNames, string toFileName)
        {
            using (var fs = new FileStream(toFileName, FileMode.Create))
            {
                var document = new Document();
                var pdfCopy = new PdfCopy(document, fs);
                document.Open();

                PdfReader reader = null;
                PdfImportedPage page = null;

                fromFileNames.ForEach(fileName =>
                {
                    reader = new PdfReader(fileName);

                    for (int i = 0; i < reader.NumberOfPages; i++)
                    {
                        page = pdfCopy.GetImportedPage(reader, i + 1);
                        pdfCopy.AddPage(page);
                    }

                    pdfCopy.FreeReader(reader);
                    reader.Close();
                    // File.Delete(fileName);
                });

                document.Close();
                fs.Close();
            }
        }

        public static void ConcatenatePdfFiles(List<string> fromFileNames, string toFileName)
        {
            LambdaLogger.Log($"toFileName:  {toFileName}\r\n");

            var pageOffset = 0;
            var master = new ArrayList();
            var f = 0;
            Document document = null/* TODO Change to default(_) if this is not a reference type */;
            PdfCopy writer = null/* TODO Change to default(_) if this is not a reference type */;

            foreach (var fromFileName in fromFileNames)
            {
                LambdaLogger.Log($"fromFileName:  {fromFileName}\r\n");

                // we create a reader for a certain document
                var reader = new PdfReader(fromFileName);
                reader.ConsolidateNamedDestinations();

                // we retrieve the total number of pages
                var numberOfPages = reader.NumberOfPages;
                var bookmarks = SimpleBookmark.GetBookmark(reader);
                if (bookmarks != null)
                {
                    if (pageOffset != 0)
                    {
                        SimpleBookmark.ShiftPageNumbers(bookmarks, pageOffset, null);
                    }
                    master.AddRange(bookmarks);
                }
                pageOffset += numberOfPages;

                if (f == 0)
                {
                    // step 1: creation of a document-object
                    document = new Document(reader.GetPageSizeWithRotation(1));

                    // step 2: we create a writer that listens to the document
                    writer = new PdfCopy(document, new FileStream(toFileName, FileMode.Create));

                    // step 3: we open the document
                    document.Open();
                }

                // step 4: we add content
                PdfImportedPage page;
                int i = 0;
                while (i < numberOfPages)
                {
                    i += 1;
                    page = writer.GetImportedPage(reader, i);
                    writer.AddPage(page);
                }

                PRAcroForm form = reader.AcroForm;
                if (form != null)
                {
                    writer.CopyAcroForm(reader);
                }

                reader.Close();
                f++;
            }

            // step 5: we close the document
            document.Close();
        }

        public static DateTime ToDate(string yyyy_mm_dd)
        {
            return new DateTime(Convert.ToInt32(yyyy_mm_dd.Substring(0, 4)), Convert.ToInt32(yyyy_mm_dd.Substring(5, 2)), Convert.ToInt32(yyyy_mm_dd.Substring(8, 2)));
        }

        public static string[] WrapLine(string lineToWrap, int maxLineLength)
        {
            return Regex.Matches(lineToWrap, @"(.{1," + maxLineLength + @"})(?:\s|$)")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
        }
    }
}
