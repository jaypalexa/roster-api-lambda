using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.Helpers
{
    public static class ConversionHelper
    {
        public static Task DoConversion(string jsonFileName) // async
        {
            return Task.CompletedTask;

            //var organizationId = "b225dd24-1d37-4252-b989-e342a690968b";

            //var basePath = AppDomain.CurrentDomain.BaseDirectory;
            //var importFileName = Path.Combine(basePath, jsonFileName);
            //var json = File.ReadAllText(importFileName);

            //var seaTurtles = JsonSerializer.Deserialize<List<SeaTurtleModel>>(json);
            //var seaTurtleService = new SeaTurtleService(organizationId);
            //foreach (var seaTurtle in seaTurtles)
            //{
            //    await seaTurtleService.SaveSeaTurtle(seaTurtle.seaTurtleId, seaTurtle);
            //}

            //var seaTurtleTags = JsonSerializer.Deserialize<List<SeaTurtleTagModel>>(json);
            //foreach (var seaTurtleTag in seaTurtleTags)
            //{
            //    var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtleTag.seaTurtleId);
            //    await seaTurtleTagService.SaveSeaTurtleTag(seaTurtleTag.seaTurtleTagId, seaTurtleTag);
            //}

            //var seaTurtleMorphometrics = JsonSerializer.Deserialize<List<SeaTurtleMorphometricModel>>(json);
            //foreach (var seaTurtleMorphometric in seaTurtleMorphometrics)
            //{
            //    var seaTurtleMorphometricService = new SeaTurtleMorphometricService(organizationId, seaTurtleMorphometric.seaTurtleId);
            //    await seaTurtleMorphometricService.SaveSeaTurtleMorphometric(seaTurtleMorphometric.seaTurtleMorphometricId, seaTurtleMorphometric);
            //}

            //var holdingTanks = JsonSerializer.Deserialize<List<HoldingTankModel>>(json);
            //var holdingTankService = new HoldingTankService(organizationId);
            //foreach (var holdingTank in holdingTanks)
            //{
            //    await holdingTankService.SaveHoldingTank(holdingTank.holdingTankId, holdingTank);
            //}

            //var holdingTankMeasurements = JsonSerializer.Deserialize<List<HoldingTankMeasurementModel>>(json);
            //foreach (var holdingTankMeasurement in holdingTankMeasurements)
            //{
            //    var holdingTankMeasurementService = new HoldingTankMeasurementService(organizationId, holdingTankMeasurement.holdingTankId);
            //    await holdingTankMeasurementService.SaveHoldingTankMeasurement(holdingTankMeasurement.holdingTankMeasurementId, holdingTankMeasurement);
            //}

            //var hatchlingsEvents = JsonSerializer.Deserialize<List<HatchlingsEventModel>>(json);
            //var hatchlingsEventService = new HatchlingsEventService(organizationId);
            //foreach (var hatchlingsEvent in hatchlingsEvents)
            //{
            //    await hatchlingsEventService.SaveHatchlingsEvent(hatchlingsEvent.hatchlingsEventId, hatchlingsEvent);
            //}

            //var washbacksEvents = JsonSerializer.Deserialize<List<WashbacksEventModel>>(json);
            //var washbacksEventService = new WashbacksEventService(organizationId);
            //foreach (var washbacksEvent in washbacksEvents)
            //{
            //    await washbacksEventService.SaveWashbacksEvent(washbacksEvent.washbacksEventId, washbacksEvent);
            //}

        }
    }
}
