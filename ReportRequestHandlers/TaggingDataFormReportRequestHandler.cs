using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class TaggingDataFormReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var reportOptions = JsonSerializer.Deserialize<TaggingDataFormReportOptionsDto>(request.body.GetRawText());

            var fileTimestamp = $"{DateTime.Now:yyyyMMddHHmmss} UTC";

            var baseMasterReportFileName = "MASTER - Tagging Data form.pdf";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            var masterReportFileName = Path.Combine(basePath, "pdf", baseMasterReportFileName);
            var filledReportFileName = Path.Combine("/tmp", baseMasterReportFileName.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));

            var organizationService = new OrganizationService(organizationId);
            var organization = await organizationService.GetOrganization();
            var organizationInformation = $"{organization.organizationName} - {organization.phone} - {organization.emailAddress}";

            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtle = await seaTurtleService.GetSeaTurtle(reportOptions.seaTurtleId);

            var seaTurtleTagService = new SeaTurtleTagService(organizationId, reportOptions.seaTurtleId);
            var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();

            var seaTurtleMorphometricService = new SeaTurtleMorphometricService(organizationId, reportOptions.seaTurtleId);
            var seaTurtleMorphometrics = await seaTurtleMorphometricService.GetSeaTurtleMorphometrics();

            var nonPitTags = seaTurtleTags.Where(x => x.tagType != "PIT" && !string.IsNullOrWhiteSpace(x.tagNumber));

            var flipperTagLeftFront = string.Join(", ", nonPitTags.Where(x => x.location == "LFF").Select(x => x.tagNumber));
            var flipperTagRightFront = string.Join(", ", nonPitTags.Where(x => x.location == "RFF").Select(x => x.tagNumber));
            var flipperTagLeftRear = string.Join(", ", nonPitTags.Where(x => x.location == "LRF").Select(x => x.tagNumber));
            var flipperTagRightRear = string.Join(", ", nonPitTags.Where(x => x.location == "RRF").Select(x => x.tagNumber));

            var pitTags = seaTurtleTags.Where(x => x.tagType == "PIT");
            var pitTagNumber = string.Join(", ", pitTags.Where(x => !string.IsNullOrWhiteSpace(x.tagNumber)).Select(x => x.tagNumber));
            var pitTagLocation = string.Join(", ", pitTags.Where(x => !string.IsNullOrWhiteSpace(x.location)).Select(x => x.location));

            //----------------------------------------------------------------------------------------------------

            var pdfReader = new PdfReader(masterReportFileName);
            pdfReader.RemoveUsageRights();

            var fs = new FileStream(filledReportFileName, FileMode.Create);
            var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

            var info = pdfReader.Info;
            info["Title"] = baseMasterReportFileName.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
            //info["Subject"] = "NEW SUBJECT";
            //info["Keywords"] = "KEYWORD1, KEYWORD2";
            //info["Creator"] = "NEW CREATOR";
            //info["Author"] = "NEW AUTHOR";
            pdfStamper.MoreInfo = info;

            var acroFields = pdfStamper.AcroFields;

            acroFields.SetField("txtSID", reportOptions.printSidOnForm ? $"SID:  {seaTurtle.sidNumber}" : string.Empty);
            acroFields.SetField("txtSpecies", seaTurtle.species);

            string dateCaptured = seaTurtle.dateCaptured ?? seaTurtle.dateAcquired;

            if (!string.IsNullOrEmpty(dateCaptured))
            {
                acroFields.SetField("txtDateCapturedDay", dateCaptured.Substring(8, 2));
                acroFields.SetField("txtDateCapturedMonth", dateCaptured.Substring(5, 2));
                acroFields.SetField("txtDateCapturedYear", dateCaptured.Substring(0, 4));
            }

            if (!string.IsNullOrEmpty(seaTurtle.dateRelinquished))
            {
                acroFields.SetField("txtDateReleasedDay", seaTurtle.dateRelinquished.Substring(8, 2));
                acroFields.SetField("txtDateReleasedMonth", seaTurtle.dateRelinquished.Substring(5, 2));
                acroFields.SetField("txtDateReleasedYear", seaTurtle.dateRelinquished.Substring(0, 4));
            }

            acroFields.SetField("txtFlipperTagLeftFront", flipperTagLeftFront);
            acroFields.SetField("txtFlipperTagRightFront", flipperTagRightFront);
            acroFields.SetField("txtFlipperTagLeftRear", flipperTagLeftRear);
            acroFields.SetField("txtFlipperTagRightRear", flipperTagRightRear);

            acroFields.SetField("txtPitTagNumber", pitTagNumber);
            acroFields.SetField("txtPitTagLocation", pitTagLocation);

            if (seaTurtle.wasCarryingTagsWhenEnc)
            {
                acroFields.SetField("radTurtleCarryingTags", "Yes");

                if (seaTurtle.recaptureType == "S")
                {
                    acroFields.SetField("radRecapture", "1");
                }
                else if (seaTurtle.recaptureType == "D")
                {
                    acroFields.SetField("radRecapture", "2");
                }
            }
            else
            {
                acroFields.SetField("radTurtleCarryingTags", "No");
            }

            acroFields.SetField("txtTagReturnAddress", seaTurtle.tagReturnAddress);

            acroFields.SetField("txtOrganizationInformation", organizationInformation);

            switch (seaTurtle.captureProjectType)
            {
                case "N":
                    acroFields.SetField("radProjectType", "NestingBeach");
                    switch (seaTurtle.didTurtleNest)
                    {
                        case "Y":
                            acroFields.SetField("radDidTurtleNest", "Yes");
                            break;
                        case "N":
                            acroFields.SetField("radDidTurtleNest", "No");
                            break;
                        case "U":
                            acroFields.SetField("radDidTurtleNest", "Undetermined");
                            break;
                        default:
                            break;
                    }
                    break;
                case "T":
                    acroFields.SetField("radProjectType", "TangleNet");
                    break;
                case "P":
                    acroFields.SetField("radProjectType", "PoundNet");
                    break;
                case "H":
                    acroFields.SetField("radProjectType", "HandCatch");
                    break;
                case "S":
                    acroFields.SetField("radProjectType", "Stranding");
                    break;
                case "O":
                    acroFields.SetField("radProjectType", "Other");
                    acroFields.SetField("txtProjectTypeOther", seaTurtle.captureProjectOther);
                    break;
                default:
                    break;
            }

            if (reportOptions.populateFacilityField)
            {
                acroFields.SetField("txtFacility", organizationInformation);
            }

            var acquiredCounty = !string.IsNullOrEmpty(seaTurtle.acquiredCounty) ? $"; County: {seaTurtle.acquiredCounty}" : string.Empty;
            var acquiredLatitude = !string.IsNullOrEmpty(seaTurtle.acquiredLatitude) ? $"; Latitude: {seaTurtle.acquiredLatitude}" : string.Empty;
            var acquiredLongitude = !string.IsNullOrEmpty(seaTurtle.acquiredLongitude) ? $"; Longitude: {seaTurtle.acquiredLongitude}" : string.Empty;
            var captureLocation = $"{seaTurtle.acquiredFrom}{acquiredCounty}{acquiredLatitude}{acquiredLongitude}".TrimStart(' ', ';');
            acroFields.SetField("txtCaptureLocation", captureLocation);

            var relinquishedCounty = !string.IsNullOrEmpty(seaTurtle.relinquishedCounty) ? $"; County: {seaTurtle.relinquishedCounty}" : string.Empty;
            var relinquishedLatitude = !string.IsNullOrEmpty(seaTurtle.relinquishedLatitude) ? $"; Latitude: {seaTurtle.relinquishedLatitude}" : string.Empty;
            var relinquishedLongitude = !string.IsNullOrEmpty(seaTurtle.relinquishedLongitude) ? $"; Longitude: {seaTurtle.relinquishedLongitude}" : string.Empty;
            var releaseLocation = $"{seaTurtle.relinquishedTo}{relinquishedCounty}{relinquishedLatitude}{relinquishedLongitude}".TrimStart(' ', ';');
            acroFields.SetField("txtReleaseLocation", releaseLocation);

            // If first string is greater than second string, it returns 1 else it returns -1.
            var dateAcquired = string.IsNullOrWhiteSpace(seaTurtle.dateAcquired) ? "0000-00-00" : seaTurtle.dateAcquired;
            var dateRelinquished = string.IsNullOrWhiteSpace(seaTurtle.dateRelinquished) ? "9999-99-99" : seaTurtle.dateRelinquished;
            var closestMorphometric = new SeaTurtleMorphometricModel();

            if (reportOptions.useMorphometricsClosestTo == "dateAcquired")
            {
                // get first after acquired...
                closestMorphometric = seaTurtleMorphometrics
                    .Where(x => string.Compare(dateAcquired, x.dateMeasured) == -1)
                    .OrderBy(x => x.dateMeasured)
                    .FirstOrDefault();

                //...if none, try earliest date
                closestMorphometric ??= seaTurtleMorphometrics
                    .OrderBy(x => x.dateMeasured)
                    .FirstOrDefault();
            }
            else //if (useMorphometricsClosestTo == "dateRelinquished")
            {
                // get first before relinquished...
                closestMorphometric = seaTurtleMorphometrics
                    .Where(x => string.Compare(dateRelinquished, x.dateMeasured) == 1)
                    .OrderByDescending(x => x.dateMeasured)
                    .FirstOrDefault();

                //...if none, try latest date
                closestMorphometric ??= seaTurtleMorphometrics
                    .OrderByDescending(x => x.dateMeasured)
                    .FirstOrDefault();
            }

            if (closestMorphometric != null)
            {
                if (closestMorphometric.sclNotchNotchValue > 0)
                {
                    if (closestMorphometric.sclNotchNotchUnits == "cm")
                    {
                        acroFields.SetField("txtSclMinCm", Convert.ToString(closestMorphometric.sclNotchNotchValue));
                    }
                    else if (closestMorphometric.sclNotchNotchUnits == "in")
                    {
                        acroFields.SetField("txtSclMinIn", Convert.ToString(closestMorphometric.sclNotchNotchValue));
                    }
                }
                if (closestMorphometric.sclNotchTipValue > 0)
                {
                    if (closestMorphometric.sclNotchTipUnits == "cm")
                    {
                        acroFields.SetField("txtSclNotchTipCm", Convert.ToString(closestMorphometric.sclNotchTipValue));
                    }
                    else if (closestMorphometric.sclNotchTipUnits == "in")
                    {
                        acroFields.SetField("txtSclNotchTipIn", Convert.ToString(closestMorphometric.sclNotchTipValue));
                    }
                }
                if (closestMorphometric.scwValue > 0)
                {
                    if (closestMorphometric.scwUnits == "cm")
                    {
                        acroFields.SetField("txtScwCm", Convert.ToString(closestMorphometric.scwValue));
                    }
                    else if (closestMorphometric.scwUnits == "in")
                    {
                        acroFields.SetField("txtScwIn", Convert.ToString(closestMorphometric.scwValue));
                    }
                }
                if (closestMorphometric.cclNotchNotchValue > 0)
                {
                    if (closestMorphometric.cclNotchNotchUnits == "cm")
                    {
                        acroFields.SetField("txtCclMinCm", Convert.ToString(closestMorphometric.cclNotchNotchValue));
                    }
                    else if (closestMorphometric.cclNotchNotchUnits == "in")
                    {
                        acroFields.SetField("txtCclMinIn", Convert.ToString(closestMorphometric.cclNotchNotchValue));
                    }
                }
                if (closestMorphometric.cclNotchTipValue > 0)
                {
                    if (closestMorphometric.cclNotchTipUnits == "cm")
                    {
                        acroFields.SetField("txtCclNotchTipCm", Convert.ToString(closestMorphometric.cclNotchTipValue));
                    }
                    else if (closestMorphometric.cclNotchTipUnits == "in")
                    {
                        acroFields.SetField("txtCclNotchTipIn", Convert.ToString(closestMorphometric.cclNotchTipValue));
                    }
                }
                if (closestMorphometric.ccwValue > 0)
                {
                    if (closestMorphometric.ccwUnits == "cm")
                    {
                        acroFields.SetField("txtCcwCm", Convert.ToString(closestMorphometric.ccwValue));
                    }
                    else if (closestMorphometric.ccwUnits == "in")
                    {
                        acroFields.SetField("txtCcwIn", Convert.ToString(closestMorphometric.ccwValue));
                    }
                }
                if (closestMorphometric.weightValue > 0)
                {
                    if (closestMorphometric.weightUnits == "kg")
                    {
                        acroFields.SetField("txtWeightKg", Convert.ToString(closestMorphometric.weightValue));
                    }
                    else if (closestMorphometric.weightUnits == "lb")
                    {
                        acroFields.SetField("txtWeightLbs", Convert.ToString(closestMorphometric.weightValue));
                    }
                }
            }

            if (seaTurtle.inspectedForTagScars)
            {
                acroFields.SetField("radTagScars", "Yes");
                acroFields.SetField("txtTagScars", seaTurtle.tagScarsLocated);
            }
            else
            {
                acroFields.SetField("radTagScars", "No");
            }

            if (seaTurtle.scannedForPitTags)
            {
                acroFields.SetField("radPitTags", "Yes");
                acroFields.SetField("txtPitTags", seaTurtle.pitTagsScanFrequency);
            }
            else
            {
                acroFields.SetField("radPitTags", "No");
            }

            if (seaTurtle.scannedForMagneticWires)
            {
                acroFields.SetField("radMagneticWires", "Yes");
                acroFields.SetField("txtMagneticWires", seaTurtle.magneticWiresLocated);
            }
            else
            {
                acroFields.SetField("radMagneticWires", "No");
            }

            if (seaTurtle.inspectedForLivingTags)
            {
                acroFields.SetField("radLivingTags", "Yes");
                acroFields.SetField("txtLivingTags", seaTurtle.livingTagsLocated);
            }
            else
            {
                acroFields.SetField("radLivingTags", "No");
            }

            acroFields.SetField("radAdditionalRemarksOnBack", reportOptions.additionalRemarksOrDataOnBackOfForm ? "Yes" : "No");

            // =============================================================================

            pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            pdfStamper.Close();
            pdfReader.Close();

            var bytes = await File.ReadAllBytesAsync(filledReportFileName);

            return bytes;
        }
    }
}
