using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.Helpers
{
    public class ConversionHelper
    {
        public static async Task DoConversion()
        {
            var organizationId = "b225dd24-1d37-4252-b989-e342a690968b";

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var importFileName = Path.Combine(basePath, "csvjson.washbacks_released_event.json");
            var json = File.ReadAllText(importFileName);

            //var seaTurtles = JsonSerializer.Deserialize<List<SeaTurtleModel>>(json);
            //var seaTurtleService = new SeaTurtleService(organizationId);
            //foreach (var seaTurtle in seaTurtles)
            //{
            //    var body = JsonSerializer.Serialize<SeaTurtleModel>(seaTurtle);
            //    await seaTurtleService.SaveSeaTurtle(seaTurtle.seaTurtleId, body);
            //}

            //var seaTurtleTags = JsonSerializer.Deserialize<List<SeaTurtleTagModel>>(json);
            //foreach (var seaTurtleTag in seaTurtleTags)
            //{
            //    var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtleTag.seaTurtleId);
            //    var body = JsonSerializer.Serialize<SeaTurtleTagModel>(seaTurtleTag);
            //    await seaTurtleTagService.SaveSeaTurtleTag(seaTurtleTag.seaTurtleTagId, body);
            //}

            //var seaTurtleMorphometrics = JsonSerializer.Deserialize<List<SeaTurtleMorphometricModel>>(json);
            //foreach (var seaTurtleMorphometric in seaTurtleMorphometrics)
            //{
            //    var seaTurtleMorphometricService = new SeaTurtleMorphometricService(organizationId, seaTurtleMorphometric.seaTurtleId);
            //    var body = JsonSerializer.Serialize<SeaTurtleMorphometricModel>(seaTurtleMorphometric);
            //    await seaTurtleMorphometricService.SaveSeaTurtleMorphometric(seaTurtleMorphometric.seaTurtleMorphometricId, body);
            //}

            //var holdingTanks = JsonSerializer.Deserialize<List<HoldingTankModel>>(json);
            //var holdingTankService = new HoldingTankService(organizationId);
            //foreach (var holdingTank in holdingTanks)
            //{
            //    var body = JsonSerializer.Serialize<HoldingTankModel>(holdingTank);
            //    await holdingTankService.SaveHoldingTank(holdingTank.holdingTankId, body);
            //}

            //var holdingTankMeasurements = JsonSerializer.Deserialize<List<HoldingTankMeasurementModel>>(json);
            //foreach (var holdingTankMeasurement in holdingTankMeasurements)
            //{
            //    var holdingTankMeasurementService = new HoldingTankMeasurementService(organizationId, holdingTankMeasurement.holdingTankId);
            //    var body = JsonSerializer.Serialize<HoldingTankMeasurementModel>(holdingTankMeasurement);
            //    await holdingTankMeasurementService.SaveHoldingTankMeasurement(holdingTankMeasurement.holdingTankMeasurementId, body);
            //}

            //var hatchlingsEvents = JsonSerializer.Deserialize<List<HatchlingsEventModel>>(json);
            //var hatchlingsEventService = new HatchlingsEventService(organizationId);
            //foreach (var hatchlingsEvent in hatchlingsEvents)
            //{
            //    var body = JsonSerializer.Serialize<HatchlingsEventModel>(hatchlingsEvent);
            //    await hatchlingsEventService.SaveHatchlingsEvent(hatchlingsEvent.hatchlingsEventId, body);
            //}

            var washbacksEvents = JsonSerializer.Deserialize<List<WashbacksEventModel>>(json);
            var washbacksEventService = new WashbacksEventService(organizationId);
            foreach (var washbacksEvent in washbacksEvents)
            {
                var body = JsonSerializer.Serialize<WashbacksEventModel>(washbacksEvent);
                await washbacksEventService.SaveWashbacksEvent(washbacksEvent.washbacksEventId, body);
            }

        }
    }
}
