using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
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
            var body = new RosterResponseBody();

            var resource = request.resource;
            var httpMethod = request.httpMethod;

            var tableName = $"roster-table-{organizationId}";

            using (var client = new AmazonDynamoDBClient())
            {
                switch (resource)
                {
                    case "/organizations/{organizationId}":
                        {
                            var key = new Dictionary<string, AttributeValue>
                                {
                                    { "PK", new AttributeValue($"ORGANIZATION#{organizationId}") },
                                    { "SK", new AttributeValue("ORGANIZATION") }
                                };
                            switch (httpMethod)
                            {
                                case "GET":
                                    {
                                        var getItemRequest = new GetItemRequest(tableName, key);
                                        var getItemResponse = await client.GetItemAsync(getItemRequest);
                                        body.data = getItemResponse.Item["data"].S;
                                        break;
                                    }
                                case "PUT":
                                    {
                                        var item = key.ToDictionary(x => x.Key, x => x.Value);
                                        item.Add("data", new AttributeValue { S = request.body });
                                        var putItemRequest = new PutItemRequest(tableName, item);
                                        var putItemResponse = await client.PutItemAsync(putItemRequest);
                                        body.data = putItemResponse;
                                        break;
                                    }
                                default: break;
                            }
                            break;
                        }

                    case "/sea-turtles":
                        {
                            switch (httpMethod)
                            {
                                case "GET":
                                    {
                                        var data = new List<string>();

                                        var hashKey = new AttributeValue { S = $"ORGANIZATION#{organizationId}" };

                                        var condition = new Condition
                                        {
                                            ComparisonOperator = "BEGINS_WITH",
                                            AttributeValueList = new List<AttributeValue>
                                            {
                                                new AttributeValue { S = "SEA_TURTLE#" }
                                            }
                                        };

                                        // Create the key conditions from hashKey and condition
                                        var keyConditions = new Dictionary<string, Condition>
                                        {
                                            // Hash key condition. ComparisonOperator must be "EQ".
                                            {
                                                "PK",
                                                new Condition
                                                {
                                                    ComparisonOperator = "EQ",
                                                    AttributeValueList = new List<AttributeValue> { hashKey }
                                                }
                                            },
                                            // Range key condition
                                            {
                                                "SK",
                                                condition
                                            }
                                        };

                                        // Define marker variable
                                        Dictionary<string, AttributeValue> startKey = null;

                                        do
                                        {
                                            var queryRequest = new QueryRequest
                                            {
                                                TableName = tableName,
                                                ExclusiveStartKey = startKey,
                                                KeyConditions = keyConditions
                                            };

                                            var queryResponse = await client.QueryAsync(queryRequest);

                                            var items = queryResponse.Items;
                                            foreach (var item in items)
                                            {
                                                foreach (var keyValuePair in item)
                                                {
                                                    if (keyValuePair.Key == "data")
                                                    {
                                                        var dataValue = keyValuePair.Value.S;
                                                        data.Add(dataValue);
                                                    }
                                                }
                                            }

                                            // Update marker variable
                                            startKey = queryResponse.LastEvaluatedKey;
                                        } while (startKey != null && startKey.Count > 0);

                                        body.data = data;
                                        break;
                                    }
                                default: break;
                            }
                            break;
                        }

                    case "/sea-turtles/{seaTurtleId}":
                        {
                            var seaTurtleId = request.pathParameters["seaTurtleId"];
                            var key = new Dictionary<string, AttributeValue>
                                {
                                    { "PK", new AttributeValue($"ORGANIZATION#{organizationId}") },
                                    { "SK", new AttributeValue($"SEA_TURTLE#{seaTurtleId}") },
                                };
                            switch (httpMethod)
                            {
                                case "GET":
                                    {
                                        var getItemRequest = new GetItemRequest(tableName, key);
                                        var getItemResponse = await client.GetItemAsync(getItemRequest);
                                        body.data = getItemResponse.Item["data"].S;
                                        break;
                                    }
                                case "PUT":
                                    {
                                        var item = key.ToDictionary(x => x.Key, x => x.Value);
                                        item.Add("data", new AttributeValue { S = request.body });
                                        var putItemRequest = new PutItemRequest(tableName, item);
                                        var putItemResponse = await client.PutItemAsync(putItemRequest);
                                        body.data = putItemResponse;
                                        break;
                                    }
                                case "DELETE":
                                    {
                                        var deleteItemRequest = new DeleteItemRequest(tableName, key);
                                        var deleteItemResponse = await client.DeleteItemAsync(deleteItemRequest);
                                        body.data = deleteItemResponse;
                                        break;
                                    }
                                default: break;
                            }
                            break;
                        }

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-tags":
                        {
                            var seaTurtleId = request.pathParameters["seaTurtleId"];
                            switch (httpMethod)
                            {
                                case "GET":
                                    {
                                        var data = new List<string>();

                                        var hashKey = new AttributeValue { S = $"SEA_TURTLE#{seaTurtleId}" };

                                        var condition = new Condition
                                        {
                                            ComparisonOperator = "BEGINS_WITH",
                                            AttributeValueList = new List<AttributeValue>
                                            {
                                                new AttributeValue { S = "SEA_TURTLE_TAG#" }
                                            }
                                        };

                                        // Create the key conditions from hashKey and condition
                                        var keyConditions = new Dictionary<string, Condition>
                                        {
                                            // Hash key condition. ComparisonOperator must be "EQ".
                                            {
                                                "PK",
                                                new Condition
                                                {
                                                    ComparisonOperator = "EQ",
                                                    AttributeValueList = new List<AttributeValue> { hashKey }
                                                }
                                            },
                                            // Range key condition
                                            {
                                                "SK",
                                                condition
                                            }
                                        };

                                        // Define marker variable
                                        Dictionary<string, AttributeValue> startKey = null;

                                        do
                                        {
                                            var queryRequest = new QueryRequest
                                            {
                                                TableName = tableName,
                                                ExclusiveStartKey = startKey,
                                                KeyConditions = keyConditions
                                            };

                                            var queryResponse = await client.QueryAsync(queryRequest);

                                            var items = queryResponse.Items;
                                            foreach (var item in items)
                                            {
                                                foreach (var keyValuePair in item)
                                                {
                                                    if (keyValuePair.Key == "data")
                                                    {
                                                        var dataValue = keyValuePair.Value.S;
                                                        data.Add(dataValue);
                                                    }
                                                }
                                            }

                                            // Update marker variable
                                            startKey = queryResponse.LastEvaluatedKey;
                                        } while (startKey != null && startKey.Count > 0);

                                        body.data = data;
                                        break;
                                    }
                                default: break;
                            }
                            break;
                        }

                    case "/sea-turtles/{seaTurtleId}/sea-turtle-tags/{seaTurtleTagId}":
                        {
                            var seaTurtleId = request.pathParameters["seaTurtleId"];
                            var seaTurtleTagId = request.pathParameters["seaTurtleTagId"];
                            var key = new Dictionary<string, AttributeValue>
                                {
                                    { "PK", new AttributeValue($"SEA_TURTLE#{seaTurtleId}") },
                                    { "SK", new AttributeValue($"SEA_TURTLE_TAG#{seaTurtleTagId}") },
                                };
                            switch (httpMethod)
                            {
                                case "GET":
                                    {
                                        var getItemRequest = new GetItemRequest(tableName, key);
                                        var getItemResponse = await client.GetItemAsync(getItemRequest);
                                        body.data = getItemResponse.Item["data"].S;
                                        break;
                                    }
                                case "PUT":
                                    {
                                        var item = key.ToDictionary(x => x.Key, x => x.Value);
                                        item.Add("data", new AttributeValue { S = request.body });
                                        var putItemRequest = new PutItemRequest(tableName, item);
                                        var putItemResponse = await client.PutItemAsync(putItemRequest);
                                        //body.data = putItemResponse;
                                        body.data = request;
                                        break;
                                    }
                                case "DELETE":
                                    {
                                        var deleteItemRequest = new DeleteItemRequest(tableName, key);
                                        var deleteItemResponse = await client.DeleteItemAsync(deleteItemRequest);
                                        body.data = deleteItemResponse;
                                        break;
                                    }
                                default: break;
                            }
                            break;
                        }

                    default: break;
                }


            }

            body.message = $"organizationId == {organizationId}";

            return body;
        }
    }
}
