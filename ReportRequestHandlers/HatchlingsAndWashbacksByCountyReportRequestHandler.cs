using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Dtos.ReportResponses;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class HatchlingsAndWashbacksByCountyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var response = new HatchlingsAndWashbacksByCountyReportContentDto();

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

            var allCountiesCount = new HatchlingsAndWashbacksByCountyReportCountyCountDto() { countyName = "ALL COUNTIES" };

            foreach (var countyName in countyNames)
            {
                var hatchlingsEventsForCounty = hatchlingsEvents.Where(x => x.eventCounty == countyName);

                int getHatchlingsEventCount(string[] species, string eventType) =>
                    hatchlingsEventsForCounty
                        .Where(x => (species.Length == 0 || species.Contains(x.species)) && x.eventType == eventType)
                        .Sum(x => x.eventCount + x.beachEventCount + x.offshoreEventCount);

                var washbacksEventsForCounty = washbacksEvents.Where(x => x.eventCounty == countyName);

                int getWashbacksEventCount(string[] species, string eventType, bool under5cmClsl) =>
                    washbacksEventsForCounty
                        .Where(x => (species.Length == 0 || species.Contains(x.species)) && x.eventType == eventType && x.under5cmClsl == under5cmClsl)
                        .Sum(x => x.eventCount + x.beachEventCount + x.offshoreEventCount);

                HatchlingsAndWashbacksByCountyReportDetailItemDto getSpeciesCounts(string[] species) => 
                    new HatchlingsAndWashbacksByCountyReportDetailItemDto
                    {
                        hatchlingsAcquired = getHatchlingsEventCount(species, "Acquired"),
                        hatchlingsDoa = getHatchlingsEventCount(species, "DOA"),
                        washbacksUnder5cmAcquired = getWashbacksEventCount(species, "Acquired", true),
                        washbacksOver5cmAcquired = getWashbacksEventCount(species, "Acquired", false),
                        washbacksUnder5cmDoa = getWashbacksEventCount(species, "DOA", true),
                        washbacksOver5cmDoa = getWashbacksEventCount(species, "DOA", false),
                    };

                var countyCount = new HatchlingsAndWashbacksByCountyReportCountyCountDto() { countyName = countyName };

                countyCount.ccCount = getSpeciesCounts(new[] { "CC" });
                countyCount.cmCount = getSpeciesCounts(new[] { "CM" });
                countyCount.dcCount = getSpeciesCounts(new[] { "DC" });
                countyCount.otherCount = getSpeciesCounts(new[] { "LK", "LO", "EI", "HB" });
                countyCount.unknownCount = getSpeciesCounts(new[] { "XX", "", null });
                countyCount.totalCount = getSpeciesCounts(new string[] { });

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
