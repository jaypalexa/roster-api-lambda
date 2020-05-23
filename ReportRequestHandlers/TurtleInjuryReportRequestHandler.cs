using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Dtos.ReportResponses.TurtleInjuryReport;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class TurtleInjuryReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var response = new ContentDto();

            var reportOptions = JsonSerializer.Deserialize<ReportOptionsDateRangeDto>(request.body.GetRawText());
            reportOptions.dateFrom ??= "0000-00-00";
            reportOptions.dateThru ??= "9999-99-99";

            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtles = (await seaTurtleService.GetSeaTurtles())
                .Where(x => !string.IsNullOrEmpty(x.dateAcquired) && x.dateAcquired.CompareTo(reportOptions.dateThru) <= 0)
                .Where(x => string.IsNullOrEmpty(x.dateRelinquished) || (!string.IsNullOrEmpty(x.dateRelinquished) && reportOptions.dateFrom.CompareTo(x.dateRelinquished) <= 0))
                .OrderBy(x => x.sidNumber)
                .ThenBy(x => x.dateAcquired)
                .ThenBy(x => x.seaTurtleName);

            response.totalCount = seaTurtles.Count();

            SummaryItemDto GetSummaryItem(string label, string propertyName)
            {
                var count = seaTurtles.Count(x => Convert.ToBoolean(x.GetType().GetProperty(propertyName).GetValue(x)));
                return new SummaryItemDto
                {
                    label = label,
                    count = count,
                    percentageOfTotal = (double)count / response.totalCount * 100
                };
            }

            response.summaryItems.Add(GetSummaryItem("Boat/Propeller strike", "injuryBoatStrike"));
            response.summaryItems.Add(GetSummaryItem("Intestinal impaction", "injuryIntestinalImpaction"));
            response.summaryItems.Add(GetSummaryItem("Line/Net entanglement", "injuryLineEntanglement"));
            response.summaryItems.Add(GetSummaryItem("Fish hook", "injuryFishHook"));
            response.summaryItems.Add(GetSummaryItem("Upper respiratory", "injuryUpperRespiratory"));
            response.summaryItems.Add(GetSummaryItem("Shark/Bird bite", "injuryAnimalBite"));
            response.summaryItems.Add(GetSummaryItem("Fibropapilloma", "injuryFibropapilloma"));
            response.summaryItems.Add(GetSummaryItem("Misc. epidemic", "injuryMiscEpidemic"));
            response.summaryItems.Add(GetSummaryItem("DOA", "injuryDoa"));
            response.summaryItems.Add(GetSummaryItem("Other", "injuryOther"));

            var noneCount = seaTurtles.Count(x => 
                !x.injuryBoatStrike && !x.injuryIntestinalImpaction && !x.injuryLineEntanglement
                && !x.injuryFishHook && !x.injuryUpperRespiratory && !x.injuryAnimalBite
                && !x.injuryFibropapilloma && !x.injuryMiscEpidemic && !x.injuryDoa && !x.injuryOther
            );

            var noneCountSummaryItem = new SummaryItemDto
            {
                label = "None",
                count = noneCount,
                percentageOfTotal = (double)noneCount / response.totalCount * 100
            };

            response.summaryItems.Add(noneCountSummaryItem);

            foreach (var seaTurtle in seaTurtles)
            {
                var detailItem = new DetailItemDto
                {
                    seaTurtleId = seaTurtle.seaTurtleId,
                    seaTurtleName = seaTurtle.seaTurtleName ?? seaTurtle.sidNumber,
                    injuryBoatStrike = seaTurtle.injuryBoatStrike,
                    injuryIntestinalImpaction = seaTurtle.injuryIntestinalImpaction,
                    injuryLineEntanglement = seaTurtle.injuryLineEntanglement,
                    injuryFishHook = seaTurtle.injuryFishHook,
                    injuryUpperRespiratory = seaTurtle.injuryUpperRespiratory,
                    injuryAnimalBite = seaTurtle.injuryAnimalBite,
                    injuryFibropapilloma = seaTurtle.injuryFibropapilloma,
                    injuryMiscEpidemic = seaTurtle.injuryMiscEpidemic,
                    injuryDoa = seaTurtle.injuryDoa,
                    injuryOther = seaTurtle.injuryOther,
                };

                response.detailItems.Add(detailItem);
            }

            return response;
        }
    }
}
