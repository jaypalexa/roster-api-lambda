using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Dtos.ReportResponses;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class TurtleTagReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var response = new TurtleTagReportContentDto();

            var reportOptions = JsonSerializer.Deserialize<TurtleTagReportOptionsDto>(request.body.GetRawText());
            reportOptions.dateFrom ??= "0000-00-00";
            reportOptions.dateThru ??= "9999-99-99";

            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtles = (await seaTurtleService.GetSeaTurtles()).OrderBy(x => x.dateAcquired).ThenBy(x => x.sidNumber).ThenBy(x => x.seaTurtleName);

            foreach (var seaTurtle in seaTurtles)
            {
                var item = new TurtleTagReportDetailItemDto
                {
                    seaTurtleId = seaTurtle.seaTurtleId,
                    sidNumber = seaTurtle.sidNumber,
                    seaTurtleName = seaTurtle.seaTurtleName,
                    dateRelinquished = seaTurtle.dateRelinquished,
                    strandingIdNumber = seaTurtle.strandingIdNumber,
                };

                var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtle.seaTurtleId);
                var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();
                seaTurtleTags = seaTurtleTags.Where(x =>
                    (reportOptions.isPit && x.tagType == "PIT")
                    || (reportOptions.isLff && x.location == "LFF")
                    || (reportOptions.isRff && x.location == "RFF")
                    || (reportOptions.isLrf && x.location == "LRF")
                    || (reportOptions.isRrf && x.location == "RRF")
                ).ToList();
                var orderedTags = seaTurtleTags.OrderBy(x => x.tagType != "PIT").ThenBy(x => x.location);
                item.tags = orderedTags.Select(x => new TurtleTagReportDetailItemTagDto { label = x.tagType == "PIT" ? "PIT" : x.location, tagNumber = x.tagNumber, dateTagged = x.dateTagged }).ToList();

                var includeItem = false;
                switch (reportOptions.filterDateType)
                {
                    case "dateTagged":
                        includeItem = item.tags.Any(x => !string.IsNullOrEmpty(x.dateTagged)
                            && (reportOptions.dateFrom.CompareTo(x.dateTagged) <= 0 && x.dateTagged.CompareTo(reportOptions.dateThru) <= 0));
                        break;
                    case "dateAcquired":
                        includeItem = string.IsNullOrEmpty(seaTurtle.dateAcquired)
                            || (reportOptions.dateFrom.CompareTo(seaTurtle.dateAcquired) <= 0 && seaTurtle.dateAcquired.CompareTo(reportOptions.dateThru) <= 0);
                        break;
                    case "dateRelinquished":
                        if (reportOptions.includeNonRelinquishedTurtles)
                        {
                            includeItem = string.IsNullOrEmpty(seaTurtle.dateRelinquished)
                                || (reportOptions.dateFrom.CompareTo(seaTurtle.dateRelinquished) <= 0 && seaTurtle.dateRelinquished.CompareTo(reportOptions.dateThru) <= 0);
                        }
                        else
                        {
                            includeItem = !string.IsNullOrEmpty(seaTurtle.dateRelinquished)
                                && (reportOptions.dateFrom.CompareTo(seaTurtle.dateRelinquished) <= 0 && seaTurtle.dateRelinquished.CompareTo(reportOptions.dateThru) <= 0);
                        }
                        break;
                    default:
                        break;
                }

                if (includeItem)
                {
                    response.detailItems.Add(item);
                }
            }

            return response;
        }
    }
}
