using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class HomeSummaryService
    {
        public async Task<HomeSummaryModel> GetHomeSummary(string organizationId)
        {
            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtleListItems = await seaTurtleService.GetSeaTurtleListItems();

            var hatchlingsEventService = new HatchlingsEventService(organizationId);
            var hatchlingsEvents = await hatchlingsEventService.GetHatchlingsEvents();

            var washbacksEventService = new WashbacksEventService(organizationId);
            var washbacksEvents = await washbacksEventService.GetWashbacksEvents();

            var holdingTankService = new HoldingTankService(organizationId);
            var holdingTanks = await holdingTankService.GetHoldingTanks();

            var homeSummary = new HomeSummaryModel
            {
                activeSeaTurtlesCount = seaTurtleListItems.Where(x => string.IsNullOrEmpty(x.dateRelinquished)).Count(),
                relinquishedSeaTurtlesCount = seaTurtleListItems.Where(x => !string.IsNullOrEmpty(x.dateRelinquished)).Count(),
                acquiredHatchlingsEventsCount = hatchlingsEvents.Where(x => x.eventType.Equals("Acquired", StringComparison.InvariantCultureIgnoreCase)).Count(),
                diedHatchlingsEventsCount = hatchlingsEvents.Where(x => x.eventType.Equals("Died", StringComparison.InvariantCultureIgnoreCase)).Count(),
                releasedHatchlingsEventsCount = hatchlingsEvents.Where(x => x.eventType.Equals("Released", StringComparison.InvariantCultureIgnoreCase)).Count(),
                doaHatchlingsEventsCount = hatchlingsEvents.Where(x => x.eventType.Equals("DOA", StringComparison.InvariantCultureIgnoreCase)).Count(),
                acquiredWashbacksEventsCount = washbacksEvents.Where(x => x.eventType.Equals("Acquired", StringComparison.InvariantCultureIgnoreCase)).Count(),
                diedWashbacksEventsCount = washbacksEvents.Where(x => x.eventType.Equals("Died", StringComparison.InvariantCultureIgnoreCase)).Count(),
                releasedWashbacksEventsCount = washbacksEvents.Where(x => x.eventType.Equals("Released", StringComparison.InvariantCultureIgnoreCase)).Count(),
                doaWashbacksEventsCount = washbacksEvents.Where(x => x.eventType.Equals("DOA", StringComparison.InvariantCultureIgnoreCase)).Count(),
                holdingTanksCount = holdingTanks.Count(),
            };

            return homeSummary;
        }
    }
}
