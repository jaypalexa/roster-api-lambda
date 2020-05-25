using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Dtos.ReportResponses.HatchlingsAndWashbacksByCountyReport;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public static class HatchlingsAndWashbacksByCountyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var response = new ContentDto();

            var reportOptions = JsonSerializer.Deserialize<ReportOptionsDateRangeDto>(request.body.GetRawText());
            reportOptions.dateFrom ??= "0000-00-00";
            reportOptions.dateThru ??= "9999-99-99";

            var hatchlingsEventService = new HatchlingsEventService(organizationId);
            var hatchlingsEvents = (await hatchlingsEventService.GetHatchlingsEvents()).Where(x => !string.IsNullOrEmpty(x.eventDate)
                && (reportOptions.dateFrom.CompareTo(x.eventDate) <= 0 && x.eventDate.CompareTo(reportOptions.dateThru) <= 0));

            var washbacksEventService = new WashbacksEventService(organizationId);
            var washbacksEvents = (await washbacksEventService.GetWashbacksEvents()).Where(x => !string.IsNullOrEmpty(x.eventDate)
                && (reportOptions.dateFrom.CompareTo(x.eventDate) <= 0 && x.eventDate.CompareTo(reportOptions.dateThru) <= 0));

            var countyNames = hatchlingsEvents.Select(x => x.eventCounty)
                .Union(washbacksEvents.Select(x => x.eventCounty))
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .OrderBy(x => x).ToList();

            var allCountiesCount = new CountyCountDto() { countyName = "ALL COUNTIES" };

            foreach (var countyName in countyNames)
            {
                var hatchlingsEventsForCounty = hatchlingsEvents.Where(x => x.eventCounty == countyName);

                int GetHatchlingsEventCount(string[] species, string eventType) =>
                    hatchlingsEventsForCounty
                        .Where(x => (species.Length == 0 || species.Contains(x.species)) && x.eventType == eventType)
                        .Sum(x => x.eventCount + x.beachEventCount + x.offshoreEventCount);

                var washbacksEventsForCounty = washbacksEvents.Where(x => x.eventCounty == countyName);

                int GetWashbacksEventCount(string[] species, string eventType, bool under5cmClsl) =>
                    washbacksEventsForCounty
                        .Where(x => (species.Length == 0 || species.Contains(x.species)) && x.eventType == eventType && x.under5cmClsl == under5cmClsl)
                        .Sum(x => x.eventCount + x.beachEventCount + x.offshoreEventCount);

                DetailItemDto GetSpeciesCounts(string[] species) =>
                    new DetailItemDto
                    {
                        hatchlingsAcquired = GetHatchlingsEventCount(species, "Acquired"),
                        hatchlingsDoa = GetHatchlingsEventCount(species, "DOA"),
                        washbacksUnder5cmAcquired = GetWashbacksEventCount(species, "Acquired", true),
                        washbacksOver5cmAcquired = GetWashbacksEventCount(species, "Acquired", false),
                        washbacksUnder5cmDoa = GetWashbacksEventCount(species, "DOA", true),
                        washbacksOver5cmDoa = GetWashbacksEventCount(species, "DOA", false),
                    };

                var countyCount = new CountyCountDto() { countyName = countyName };

                countyCount.ccCount = GetSpeciesCounts(ReportHelper.speciesCc);
                countyCount.cmCount = GetSpeciesCounts(ReportHelper.speciesCm);
                countyCount.dcCount = GetSpeciesCounts(ReportHelper.speciesDc);
                countyCount.otherCount = GetSpeciesCounts(ReportHelper.speciesOther);
                countyCount.unknownCount = GetSpeciesCounts(ReportHelper.speciesUnknown);
                countyCount.totalCount = GetSpeciesCounts(new string[] { });

                response.countyCounts.Add(countyCount);

                allCountiesCount.AppendCounts(countyCount);
            }

            foreach (var countyCount in response.countyCounts)
            {
                countyCount.SetPercentageOfGrandTotal(allCountiesCount.totalCount);
            }

            response.countyCounts.Insert(0, allCountiesCount);

            return response;
        }
    }
}
