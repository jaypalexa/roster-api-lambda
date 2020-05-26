using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutoMapper;
using iTextSharp.text.pdf;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Extensions;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public static class MarineTurtleHoldingFacilityQuarterlyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            const string baseMasterReportFileName_Page1 = "MASTER - Marine Turtle Holding Facility Quarterly Report Page 1.pdf";
            const string baseMasterReportFileName_Page2 = "MASTER - Marine Turtle Holding Facility Quarterly Report Page 2.pdf";
            const string baseMasterReportFileName_Page3 = "MASTER - Marine Turtle Holding Facility Quarterly Report Page 3.pdf";

            const int PAGE_1_LINES_PER_PAGE = 8;
            const int PAGE_2_LINES_PER_PAGE = 22;
            const int PAGE_3_LINES_PER_PAGE = 34;

            var filledReportFileNames = new List<string>();
            var fileTimestamp = $"{DateTime.Now:yyyyMMddHHmmss} UTC";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            PdfReader pdfReader;

            var reportOptions = JsonSerializer.Deserialize<MarineTurtleHoldingFacilityQuarterlyReportOptionsDto>(request.body.GetRawText());
            reportOptions.dateFrom ??= "0000-00-00";
            reportOptions.dateThru ??= "9999-99-99";

            var organizationService = new OrganizationService(organizationId);
            var organization = await organizationService.GetOrganization();
            var organizationAndPermitNumber = $"{organization.organizationName} - {organization.permitNumber}";

            string monthsAndYearOfReport;
            var dateFrom = ReportHelper.ToDate(reportOptions.dateFrom);
            var dateThru = ReportHelper.ToDate(reportOptions.dateThru);

            if (dateFrom.Year == dateThru.Year)
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }
            else
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} {dateFrom.Year} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }

            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtles = (await seaTurtleService.GetSeaTurtles())
                .Where(x => !string.IsNullOrEmpty(x.dateAcquired) && x.dateAcquired.CompareTo(reportOptions.dateThru) <= 0)
                .Where(x => string.IsNullOrEmpty(x.dateRelinquished) || (!string.IsNullOrEmpty(x.dateRelinquished) && reportOptions.dateFrom.CompareTo(x.dateRelinquished) <= 0))
                .OrderBy(x => x.sidNumber)
                .ThenBy(x => x.dateAcquired)
                .ThenBy(x => x.seaTurtleName);

            var seaTurtleMapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SeaTurtleModel, HoldingFacilitySeaTurtleReportItem>();
            });
            var seaTurtleMapper = new Mapper(seaTurtleMapperConfiguration);

            var seaTurtleReportItems = new List<HoldingFacilitySeaTurtleReportItem>();

            foreach (var seaTurtle in seaTurtles)
            {
                //'----------------------------------------------------------------
                //'-- kludge to account for all the data we want to cram into the 
                //'-- status/tag number line...ugh...
                //'----------------------------------------------------------------
                var reportTagNumberFieldData = await GetReportTagNumberFieldData(organizationId, seaTurtle, reportOptions);
                var lines = ReportHelper.WrapLine(reportTagNumberFieldData, 92);
                for (int i = 0; i < lines.Length; i++)
                {
                    var item = new HoldingFacilitySeaTurtleReportItem();
                    if (i == 0)
                    {
                        item = seaTurtleMapper.Map<HoldingFacilitySeaTurtleReportItem>(seaTurtle);
                    }
                    item.reportTagNumberFieldData = lines[i];
                    seaTurtleReportItems.Add(item);
                }
            }

            var holdingTankService = new HoldingTankService(organizationId);
            var holdingTanks = await holdingTankService.GetHoldingTanks();
            var holdingTankMeasurementReportItems = new List<HoldingFacilityHoldingTankMeasurementReportItem>();

            foreach (var holdingTank in holdingTanks)
            {
                var holdingTankMeasurementMapperConfiguration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<HoldingTankMeasurementModel, HoldingFacilityHoldingTankMeasurementReportItem>()
                        .ConstructUsing(x => new HoldingFacilityHoldingTankMeasurementReportItem(holdingTank.holdingTankName));
                });
                var holdingTankMeasurementMapper = new Mapper(holdingTankMeasurementMapperConfiguration);

                var holdingTankMeasurementService = new HoldingTankMeasurementService(organizationId, holdingTank.holdingTankId);
                var items = (await holdingTankMeasurementService.GetHoldingTankMeasurements())
                    .Where(x => reportOptions.dateFrom.CompareTo(x.dateMeasured) <= 0 && x.dateMeasured.CompareTo(reportOptions.dateThru) <= 0)
                    .Select(x => holdingTankMeasurementMapper.Map<HoldingFacilityHoldingTankMeasurementReportItem>(x))
                    ;

                holdingTankMeasurementReportItems.AddRange(items);
            }

            if (reportOptions.groupTankDataBy == "tank")
            {
                holdingTankMeasurementReportItems = holdingTankMeasurementReportItems
                    .OrderBy(x => x.holdingTankName)
                    .ThenBy(x => x.dateMeasured).ToList();
            }
            else
            {
                holdingTankMeasurementReportItems = holdingTankMeasurementReportItems
                    .OrderBy(x => x.dateMeasured)
                    .ThenBy(x => x.holdingTankName).ToList();
            }

            //-- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] --

            var masterReportFileName_Page1 = Path.Combine(basePath, "pdf", baseMasterReportFileName_Page1);
            var filledReportFileName_Page1 = Path.Combine("/tmp", baseMasterReportFileName_Page1.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));
            filledReportFileNames.Add(filledReportFileName_Page1);

            pdfReader = new PdfReader(masterReportFileName_Page1);
            pdfReader.RemoveUsageRights();

            using (var fs = new FileStream(filledReportFileName_Page1, FileMode.Create))
            {
                var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

                var info = pdfReader.Info;
                info["Title"] = baseMasterReportFileName_Page1.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
                pdfStamper.MoreInfo = info;

                var acroFields = pdfStamper.AcroFields;

                acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
                acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

                var pageOneItems = seaTurtleReportItems.Take(PAGE_1_LINES_PER_PAGE).ToList();
                for (int i = 0; i < pageOneItems.Count; i++)
                {
                    var item = pageOneItems[i];
                    FillSectionOneRow(acroFields, (i + 1).ToString().PadLeft(2, '0'), item, reportOptions);
                }

                pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
                pdfStamper.Close();
            }
            pdfReader.Close();

            //-- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] --

            var page2Items = seaTurtleReportItems.Skip(PAGE_1_LINES_PER_PAGE).ToList().ChunkBy(PAGE_2_LINES_PER_PAGE);

            for (int chunkIndex = 0; chunkIndex < page2Items.Count; chunkIndex++)
            {
                var masterReportFileName_Page2 = Path.Combine(basePath, "pdf", baseMasterReportFileName_Page2);
                var filledReportFileName_Page2 = Path.Combine("/tmp", baseMasterReportFileName_Page2.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp} - {chunkIndex.ToString().PadLeft(2, '0')}.pdf"));
                filledReportFileNames.Add(filledReportFileName_Page2);

                pdfReader = new PdfReader(masterReportFileName_Page2);
                pdfReader.RemoveUsageRights();

                using (var fs = new FileStream(filledReportFileName_Page2, FileMode.Create))
                {
                    var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

                    var info = pdfReader.Info;
                    info["Title"] = baseMasterReportFileName_Page2.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
                    pdfStamper.MoreInfo = info;

                    var acroFields = pdfStamper.AcroFields;

                    acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
                    acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

                    for (int i = 0; i < page2Items[chunkIndex].Count; i++)
                    {
                        var item = page2Items[chunkIndex][i];
                        FillSectionOneRow(acroFields, (i + 1 + PAGE_1_LINES_PER_PAGE).ToString().PadLeft(2, '0'), item, reportOptions);
                    }

                    pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
                    pdfStamper.Close();
                }
                pdfReader.Close();
            }

            //-- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] --

            var page3Items = holdingTankMeasurementReportItems.ChunkBy(PAGE_3_LINES_PER_PAGE);

            for (int chunkIndex = 0; chunkIndex < page3Items.Count; chunkIndex++)
            {
                var masterReportFileName_Page3 = Path.Combine(basePath, "pdf", baseMasterReportFileName_Page3);
                var filledReportFileName_Page3 = Path.Combine("/tmp", baseMasterReportFileName_Page3.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp} - {chunkIndex.ToString().PadLeft(2, '0')}.pdf"));
                filledReportFileNames.Add(filledReportFileName_Page3);

                pdfReader = new PdfReader(masterReportFileName_Page3);
                pdfReader.RemoveUsageRights();

                using (var fs = new FileStream(filledReportFileName_Page3, FileMode.Create))
                {
                    var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

                    var info = pdfReader.Info;
                    info["Title"] = baseMasterReportFileName_Page3.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
                    pdfStamper.MoreInfo = info;

                    var acroFields = pdfStamper.AcroFields;

                    acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
                    acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

                    for (int i = 0; i < page3Items[chunkIndex].Count; i++)
                    {
                        var item = page3Items[chunkIndex][i];
                        var fieldNumber = (i + 1).ToString().PadLeft(2, '0');
                        acroFields.SetField($"txtDate{fieldNumber}", item.dateMeasured);
                        acroFields.SetField($"txtTank{fieldNumber}", item.holdingTankName);
                        acroFields.SetField($"txtTemperature{fieldNumber}", item.temperature);
                        acroFields.SetField($"txtSalinity{fieldNumber}", item.salinity);
                        acroFields.SetField($"txtPH{fieldNumber}", item.ph);
                    }

                    pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
                    pdfStamper.Close();
                }
                pdfReader.Close();
            }

            // =========================================================================================================================

            const string masterReportFileName_Final = "MASTER - Marine Turtle Holding Facility Quarterly Report.pdf";
            var filledReportFileName_Final = Path.Combine("/tmp", masterReportFileName_Final.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));

            ReportHelper.ConcatenatePdfFiles(filledReportFileNames, filledReportFileName_Final);

            var bytes = await File.ReadAllBytesAsync(filledReportFileName_Final);

            return bytes;
        }

        private static async Task<string> GetReportTagNumberFieldData(string organizationId, SeaTurtleModel seaTurtle, MarineTurtleHoldingFacilityQuarterlyReportOptionsDto reportOptions)
        {
            var sb = new StringBuilder();

            var dateFrom = ReportHelper.ToDate(reportOptions.dateFrom);
            var dateThru = ReportHelper.ToDate(reportOptions.dateThru);

            //----------------------------------------------------------------
            //-- display DATE ACQUIRED -only- when requested 
            //-- and when report date range is for one quarter or less
            //----------------------------------------------------------------
            //-- if the DATE ACQUIRED is within the date range of the report, 
            //-- then do display the ACQUIRED FROM information
            //----------------------------------------------------------------
            if (reportOptions.includeAcquiredFrom && !string.IsNullOrEmpty(seaTurtle.dateAcquired) && ((dateThru.Date - dateFrom.Date).Days <= 95))
            {
                if ((reportOptions.dateFrom.CompareTo(seaTurtle.dateAcquired) <= 0) && (seaTurtle.dateAcquired.CompareTo(reportOptions.dateThru) <= 0))
                {
                    if (!string.IsNullOrEmpty(seaTurtle.acquiredFrom))
                    {
                        sb.Append($"Acq. from: {seaTurtle.acquiredFrom}; ");
                    }
                }
            }

            var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtle.seaTurtleId);
            var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();

            if (seaTurtleTags.Count > 0)
            {
                sb.Append($"Tags: {string.Join(", ", seaTurtleTags.Select(x => x.tagNumber))}; ");
            }

            if (reportOptions.includeAnomalies && !string.IsNullOrEmpty(seaTurtle.anomalies))
            {
                sb.Append($"Anomalies: {seaTurtle.anomalies}; ");
            }

            return sb.ToString();
        }

        private static void FillSectionOneRow(AcroFields acroFields, string fieldNumber, HoldingFacilitySeaTurtleReportItem item, MarineTurtleHoldingFacilityQuarterlyReportOptionsDto reportOptions)
        {
            // always set the status/tag number field as it may be a partial line
            acroFields.SetField($"txtTagNumber{fieldNumber}", item.reportTagNumberFieldData);

            // if NOT a partial line...
            if (!string.IsNullOrEmpty(item.seaTurtleId))
            {
                // *************************************************************************************************
                // -- Tag Numbers, Acquired From, and Anomalies share the TAG NUMBER field (part of the STATUS field)
                // -- Relinquished To and Stranding ID Number share the RELINQUISHED TO field (part of the DATE RELEASED/.../... field)
                // *************************************************************************************************

                var sidNumberAndSeaTurtleName = item.sidNumber;
                if (reportOptions.includeTurtleName && !string.IsNullOrEmpty(item.seaTurtleName))
                {
                    sidNumberAndSeaTurtleName += $" - {item.seaTurtleName}";
                }
                acroFields.SetField($"txtSID{fieldNumber}", sidNumberAndSeaTurtleName);

                acroFields.SetField($"cboSpecies{fieldNumber}", item.species);

                acroFields.SetField($"txtDateAcquired{fieldNumber}", item.dateAcquired);

                acroFields.SetField($"cboSize{fieldNumber}", item.turtleSize);
                acroFields.SetField($"cboStatus{fieldNumber}", item.status);

                // ----------------------------------------------------------------
                // -- if the DATE RELINQUISHED is later than the report date, 
                // -- then do NOT display the relinquished information
                // ----------------------------------------------------------------
                var showRelinquishedInfo = item.dateRelinquished.CompareTo(reportOptions.dateThru) <= 0;

                acroFields.SetField($"txtDateRelinquished{fieldNumber}", showRelinquishedInfo ? item.dateRelinquished : string.Empty);

                // *************************************************************************************************
                // v-- *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO 
                // *************************************************************************************************
                // ----------------------------------------------------------------
                // -- add STRANDING ID NUMBER information here (if any)
                // ----------------------------------------------------------------
                var txtRelinquishedTo = string.Empty;

                if (!string.IsNullOrEmpty(item.strandingIdNumber))
                {
                    txtRelinquishedTo += $"Stranding ID #: {item.strandingIdNumber};";
                }

                if (showRelinquishedInfo && !string.IsNullOrEmpty(item.relinquishedTo))
                {
                    txtRelinquishedTo += $"Relinq. To: {item.relinquishedTo}";
                }

                acroFields.SetField($"txtRelinquishedTo{fieldNumber}", txtRelinquishedTo);
                // *************************************************************************************************
                // ^-- *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO 
                // *************************************************************************************************
            }
        }
    }

    public class HoldingFacilitySeaTurtleReportItem
    {
        public string seaTurtleId { get; set; }
        public string seaTurtleName { get; set; }
        public string sidNumber { get; set; }
        public string strandingIdNumber { get; set; }
        public string species { get; set; }
        public string dateAcquired { get; set; }
        public string acquiredFrom { get; set; }
        public string acquiredCounty { get; set; }
        public string turtleSize { get; set; }
        public string status { get; set; }
        public string dateRelinquished { get; set; }
        public string relinquishedTo { get; set; }
        public string reportTagNumberFieldData { get; set; }
    }

    public class HoldingFacilityHoldingTankMeasurementReportItem
    {
        public string holdingTankMeasurementId { get; set; }
        public string holdingTankId { get; set; }
        public string holdingTankName { get; set; }
        public string dateMeasured { get; set; }
        public double temperature { get; set; }
        public double salinity { get; set; }
        public double ph { get; set; }

        public HoldingFacilityHoldingTankMeasurementReportItem(string holdingTankName)
        {
            this.holdingTankName = holdingTankName;
        }
    }
}
