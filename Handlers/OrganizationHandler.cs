using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using RosterApiLambda.Dtos;
using RosterApiLambda.Handlers.Interfaces;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Handlers
{
    public class OrganizationHandler : IHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var dataHelper = new DataHelper(organizationId);

            using (var client = new AmazonDynamoDBClient())
            {
                var pk = $"ORGANIZATION#{organizationId}";
                var sk = "ORGANIZATION";
                return request.httpMethod switch
                {
                    "GET" => await dataHelper.GetItemAsync(pk, sk),
                    "PUT" => await dataHelper.PutItemAsync(pk, sk, request.body),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                };
            }
        }
    }
}
