using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    public class MarineTurtleCaptiveFacilityQuarterlyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var reportOptions = JsonSerializer.Deserialize<MarineTurtleCaptiveFacilityQuarterlyReportOptionsDto>(request.body.GetRawText());

            var fileTimestamp = $"{DateTime.Now:yyyyMMddHHmmss} UTC";

            var baseMasterReportFileName = $"MASTER - Marine Turtle Captive Facility Quarterly Report For {reportOptions.subjectType}.pdf";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            var masterReportFileName = Path.Combine(basePath, "pdf", baseMasterReportFileName);
            var filledReportFileName = Path.Combine("/tmp", baseMasterReportFileName.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));

            var organizationService = new OrganizationService(organizationId);
            var organization = await organizationService.GetOrganization();
            var organizationAndPermitNumber = $"{organization.organizationName} - {organization.permitNumber}";

            string monthsAndYearOfReport;
            var dateFrom = new DateTime(Convert.ToInt32(reportOptions.dateFrom.Substring(0, 4)), Convert.ToInt32(reportOptions.dateFrom.Substring(5, 2)), Convert.ToInt32(reportOptions.dateFrom.Substring(8, 2)));
            var dateThru = new DateTime(Convert.ToInt32(reportOptions.dateThru.Substring(0, 4)), Convert.ToInt32(reportOptions.dateThru.Substring(5, 2)), Convert.ToInt32(reportOptions.dateThru.Substring(8, 2)));

            if (dateFrom.Year == dateThru.Year)
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }
            else
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} {dateFrom.Year} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }

            var balanceAsOfDate = reportOptions.subjectType == "Hatchlings" ? organization.hatchlingsBalanceAsOfDate : organization.washbacksBalanceAsOfDate;
            var useOrganizationStartingBalances = !string.IsNullOrEmpty(balanceAsOfDate) && balanceAsOfDate.CompareTo(reportOptions.dateFrom) <= 0;

            var items = new Dictionary<string, CaptiveFacilityReportItem>
            {
                { "Cc", new CaptiveFacilityReportItem(ReportHelper.speciesCc) },
                { "Cm", new CaptiveFacilityReportItem(ReportHelper.speciesCm) },
                { "Dc", new CaptiveFacilityReportItem(ReportHelper.speciesDc) },
                { "Other", new CaptiveFacilityReportItem(ReportHelper.speciesOther) },
                { "Unknown", new CaptiveFacilityReportItem(ReportHelper.speciesUnknown) }
            };
            var categories = items.Keys;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<HatchlingsEventModel, CaptiveFacilityReportEvent>();
                cfg.CreateMap<WashbacksEventModel, CaptiveFacilityReportEvent>();
            });
            var mapper = new Mapper(config);

            var hatchlingsEventService = new HatchlingsEventService(organizationId);
            var washbacksEventService = new WashbacksEventService(organizationId);
            var events = reportOptions.subjectType == "Hatchlings"
                ? (await hatchlingsEventService.GetHatchlingsEvents()).Select(x => mapper.Map<CaptiveFacilityReportEvent>(x))
                : (await washbacksEventService.GetWashbacksEvents()).Select(x => mapper.Map<CaptiveFacilityReportEvent>(x));

            int GetCountsPriorToThisPeriod(string[] eventTypes, string[] species) =>
                events
                   .Where(x => eventTypes.Contains(x.eventType) && species.Contains(x.species))
                   .Where(x => x.eventDate.CompareTo(reportOptions.dateFrom) <= 0)
                   .Where(x => !string.IsNullOrEmpty(balanceAsOfDate) && balanceAsOfDate.CompareTo(x.eventDate) <= 0)
                   .Sum(x => x.eventCount + x.beachEventCount + x.offshoreEventCount);

            int GetCountsForThisPeriod(string eventType, string[] species, string eventCountType = null) =>
                events
                   .Where(x => x.eventType == eventType && species.Contains(x.species))
                   .Where(x => reportOptions.dateFrom.CompareTo(x.eventDate) <= 0)
                   .Where(x => x.eventDate.CompareTo(reportOptions.dateThru) <= 0)
                   .Sum(x => eventCountType == "beachEventCount" ? x.beachEventCount : (eventCountType == "offshoreEventCount" ? x.offshoreEventCount : x.eventCount));

            int GetStartingBalance(string category) =>
                useOrganizationStartingBalances
                    ? Convert.ToInt32(organization.GetType().GetProperty($"{category.ToLower()}{reportOptions.subjectType}StartingBalance").GetValue(organization))
                    : 0;

            //----------------------------------------------------------------------------------------------------

            var pdfReader = new PdfReader(masterReportFileName);
            pdfReader.RemoveUsageRights();

            var fs = new FileStream(filledReportFileName, FileMode.Create);
            var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

            var info = pdfReader.Info;
            info["Title"] = baseMasterReportFileName.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
            pdfStamper.MoreInfo = info;

            var acroFields = pdfStamper.AcroFields;

            acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
            acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

            var sbComments = new StringBuilder();

            foreach (var category in categories)
            {
                var item = items[category];

                item.StartingBalance = GetStartingBalance(category);

                item.AdditionsBeforeThisPeriod = GetCountsPriorToThisPeriod(new[] { "Acquired" }, item.SpeciesSelector);
                item.SubtractionsBeforeThisPeriod = GetCountsPriorToThisPeriod(new[] { "Died", "Released" }, item.SpeciesSelector);
                item.AcquiredThisPeriod = GetCountsForThisPeriod("Acquired", item.SpeciesSelector);
                item.DiedThisPeriod = GetCountsForThisPeriod("Died", item.SpeciesSelector);
                item.ReleasedOnTheBeachThisPeriod = GetCountsForThisPeriod("Released", item.SpeciesSelector, "beachEventCount");
                item.ReleasedOffshoreThisPeriod = GetCountsForThisPeriod("Released", item.SpeciesSelector, "offshoreEventCount");
                item.DoaThisPeriod = GetCountsForThisPeriod("DOA", item.SpeciesSelector);

                item.PreviousBalance = item.StartingBalance + item.AdditionsBeforeThisPeriod - item.SubtractionsBeforeThisPeriod;
                acroFields.SetField($"txt{category}PrevBal", item.PreviousBalance);
                acroFields.SetField($"txt{category}Acquired", item.AcquiredThisPeriod);
                acroFields.SetField($"txt{category}Died", item.DiedThisPeriod);
                acroFields.SetField($"txt{category}Released", item.ReleasedOnTheBeachThisPeriod + item.ReleasedOffshoreThisPeriod);
                acroFields.SetField($"txt{category}EndBal", item.PreviousBalance + item.AcquiredThisPeriod - item.DiedThisPeriod - item.ReleasedOnTheBeachThisPeriod - item.ReleasedOffshoreThisPeriod);
                acroFields.SetField($"txt{category}BeachVsOffshore", $"Beach: {item.ReleasedOnTheBeachThisPeriod}{Environment.NewLine}Offshore: {item.ReleasedOffshoreThisPeriod}");

                if (reportOptions.includeDoaCounts)
                {
                    sbComments.AppendLine($"DOA {category} hatchlings = {item.DoaThisPeriod}");
                }
            }

            sbComments.AppendLine(reportOptions.comments);
            acroFields.SetField("txtComments", sbComments.ToString());

            // =============================================================================

            pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            pdfStamper.Close();
            pdfReader.Close();

            var bytes = await File.ReadAllBytesAsync(filledReportFileName);

            return bytes;
        }
    }

    public class CaptiveFacilityReportItem
    {
        public string[] SpeciesSelector { get; }
        public int StartingBalance { get; set; }
        public int AdditionsBeforeThisPeriod { get; set; }
        public int SubtractionsBeforeThisPeriod { get; set; }
        public int AcquiredThisPeriod { get; set; }
        public int DiedThisPeriod { get; set; }
        public int ReleasedOnTheBeachThisPeriod { get; set; }
        public int ReleasedOffshoreThisPeriod { get; set; }
        public int DoaThisPeriod { get; set; }
        public int PreviousBalance { get; set; }

        public CaptiveFacilityReportItem(string[] speciesSelector)
        {
            SpeciesSelector = speciesSelector;
        }
    }

    public class CaptiveFacilityReportEvent
    {
        public string eventType { get; set; }
        public string species { get; set; }
        public string eventDate { get; set; }
        public int eventCount { get; set; }
        public int beachEventCount { get; set; }
        public int offshoreEventCount { get; set; }
    }
}
