using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using iTextSharp.text.pdf;
using Microsoft.IdentityModel.Tokens;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RosterApiLambda
{
    public class RosterRequest
    {
        public string resource { get; set; }                // ??? /root/child
        public string httpMethod { get; set; }              // DELETE|GET|HEAD|OPTIONS|PATCH|POST|PUT
        public Dictionary<string, string> headers { get; set; }                // { "headerName": "headerValue", ... }
        public Dictionary<string, string> queryStringParameters { get; set; }  // { "key": "value", ... }
        public Dictionary<string, string> pathParameters { get; set; }         // { "key": "value", ... }
        public string body { get; set; }                    // JSON.stringified
        public bool isBase64Encoded { get; set; }           // true|false
    };

    /* AWS LAMBDA INTEGRATION RESPONSE FORMAT
      {
        "cookies" : ["cookie1", "cookie2"]
        "isBase64Encoded": true|false,
        "statusCode": httpStatusCode,
        "headers": { "headerName": "headerValue", ... },
        "body": "Hello from Lambda!"
      }   
    */

    public class RosterResponseBody
    {
        public string message { get; set; }
        public object data { get; set; }
    }

    public class RosterResponse
    {
        public int statusCode { get; set; }
        public RosterResponseBody body { get; set; }
    }

    public class Function
    {
        public async Task<RosterResponse> FunctionHandler(RosterRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"REQUEST:  {request}");

            var message = string.Empty;
            object data = request;

            var response = new RosterResponse
            {
                statusCode = 200,
                body = new RosterResponseBody { data = data, message = message }
            };

            var resource = request.resource;

            if (resource == "/wake-up")
            {
                response.body.message = $"{resource} at: {DateTime.Now.ToUniversalTime()}";
                return response;
            }

            var jwt = request.headers["jwt"];
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            if (jwtSecurityTokenHandler.CanReadToken(jwt))
            {
                var decodedJwt = jwtSecurityTokenHandler.ReadJwtToken(jwt);
                var organizationId = Convert.ToString(decodedJwt.Payload["custom:organizationId"]);

                if (resource.StartsWith("/reports"))
                {
                    response.body = DoReports(organizationId);
                }
                else
                {
                    response.body = await DoData(organizationId, request);
                }
            }
            else
            {
                throw new SecurityTokenValidationException("Unable to read JWT.");
            }

            response.body.message = message;

            return response;
        }

        public RosterResponseBody DoReports(string organizationId)
        {
            var body = new RosterResponseBody();

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var fieldsReportName = Path.Combine(basePath, "test.pdf");
            //var filledReportName = fieldsReportName.Replace("MASTER - ", "FILLED - ").Replace(".master.pdf", $" - {DateTime.Now:yyyyMMddHHmmss}.pdf");
            var filledReportName = Path.Combine("/tmp", "test-filled.pdf");

            var pdfReader = new PdfReader(fieldsReportName);
            var fs = new FileStream(filledReportName, FileMode.Create);

            var pdfStamper = new PdfStamper(pdfReader, fs, '\0', true);
            var acroFields = pdfStamper.AcroFields;
            acroFields.SetField("txtTurtlePermitNumber", "JPA PERMIT NUMBER");

            // pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            pdfStamper.Close();
            pdfReader.Close();

            var bytes = File.ReadAllBytes(filledReportName);

            //var base64 = Convert.ToBase64String(bytes); // converting to base64 seems to corrupt (?) and gets wrapped in double quotes?
            //LambdaLogger.Log(base64);

            body.data = bytes;

            return body;
        }

        public async Task<RosterResponseBody> DoData(string organizationId, RosterRequest request)
        {
            string pk;
            string sk;
            var errorMessage = $"ERROR in {nameof(Function.DoData)}:  Unexpected 'httpMethod' value of '{request.httpMethod}' for resource '{request.resource}'";

            var body = new RosterResponseBody
            {
                message = $"organizationId == {organizationId}"
            };

            var dataHelper = new DataHelper(organizationId);

            using (var client = new AmazonDynamoDBClient())
            {
                switch (request.resource)
                {
                    case "/organizations/{organizationId}":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = "ORGANIZATION";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = "SEA_TURTLE#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles/{seaTurtleId}":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = $"SEA_TURTLE#{request.pathParameters["seaTurtleId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-tags":
                        pk = $"SEA_TURTLE#{request.pathParameters["seaTurtleId"]}";
                        sk = "SEA_TURTLE_TAG#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-tags/{seaTurtleTagId}":
                        pk = $"SEA_TURTLE#{request.pathParameters["seaTurtleId"]}";
                        sk = $"SEA_TURTLE_TAG#{request.pathParameters["seaTurtleTagId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics":
                        pk = $"SEA_TURTLE#{request.pathParameters["seaTurtleId"]}";
                        sk = "SEA_TURTLE_MORPHOMETRIC#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics/{seaTurtleMorphometricId}":
                        pk = $"SEA_TURTLE#{request.pathParameters["seaTurtleId"]}";
                        sk = $"SEA_TURTLE_MORPHOMETRIC#{request.pathParameters["seaTurtleMorphometricId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/holding-tanks":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = "HOLDING_TANK#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/holding-tanks/{holdingTankId}":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = $"HOLDING_TANK#{request.pathParameters["holdingTankId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/holding-tanks/{holdingTankId}/holding-tank-measurements":
                        pk = $"HOLDING_TANK#{request.pathParameters["holdingTankId"]}";
                        sk = "HOLDING_TANK_MEASUREMENT#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/holding-tanks/{holdingTankId}/holding-tank-measurements/{holdingTankMeasurementId}":
                        pk = $"HOLDING_TANK#{request.pathParameters["holdingTankId"]}";
                        sk = $"HOLDING_TANK_MEASUREMENT#{request.pathParameters["holdingTankMeasurementId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/hatchlings-events":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = "HATCHLINGS_EVENT#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/hatchlings-events/{hatchlingsEventId}":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = $"HATCHLINGS_EVENT#{request.pathParameters["hatchlingsEventId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/washbacks-events":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = "WASHBACKS_EVENT#";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.QueryAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    case "/washbacks-events/{washbacksEventId}":
                        pk = $"ORGANIZATION#{organizationId}";
                        sk = $"WASHBACKS_EVENT#{request.pathParameters["washbacksEventId"]}";
                        body.data = request.httpMethod switch
                        {
                            "GET" => await dataHelper.GetItemAsync(pk, sk),
                            "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                            "DELETE" => await dataHelper.DeleteItemAsync(pk, sk),
                            _ => throw new NotImplementedException(errorMessage),
                        };
                        break;

                    default: break;
                }
            }


            return body;
        }
    }
}
