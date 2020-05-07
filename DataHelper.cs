using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace RosterApiLambda
{
    public class DataHelper
    {
        private string TABLE_NAME { get; }

        public DataHelper(string organizationId)
        {
            TABLE_NAME = $"roster-table-{organizationId}";
        }

        private Dictionary<string, AttributeValue> GetKey(string pk, string sk) => new Dictionary<string, AttributeValue> {
            { "PK", new AttributeValue(pk) }, { "SK", new AttributeValue(sk) }
        };

        private Dictionary<string, Condition> GetKeyConditions(string pk, string sk) => new Dictionary<string, Condition> {
            { 
                "PK", new Condition {
                    ComparisonOperator = "EQ",  // ComparisonOperator must be "EQ" for PK
                    AttributeValueList = new List<AttributeValue> { new AttributeValue { S = pk } }
                } 
            },
            { 
                "SK", new Condition {
                    ComparisonOperator = "BEGINS_WITH",
                    AttributeValueList = new List<AttributeValue> { new AttributeValue { S = sk } }
                } 
            }
        };

        async public Task<object> QueryAsync(string pk, string sk)
        {
            var data = new List<string>();

            using (var client = new AmazonDynamoDBClient())
            {
                // Define marker variable
                Dictionary<string, AttributeValue> startKey = null;

                do
                {
                    var queryRequest = new QueryRequest
                    {
                        TableName = TABLE_NAME,
                        ExclusiveStartKey = startKey,
                        KeyConditions = GetKeyConditions(pk, sk)
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

                return data;
            }
        }

        async public Task<object> GetItemAsync(string pk, string sk)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var key = GetKey(pk, sk);
                var getItemRequest = new GetItemRequest(TABLE_NAME, key);
                var getItemResponse = await client.GetItemAsync(getItemRequest);
                return getItemResponse.Item["data"].S;
            }
        }

        async public Task<object> PutItemAsync(string pk, string sk, string body)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var key = GetKey(pk, sk);
                var item = key.ToDictionary(x => x.Key, x => x.Value);
                item.Add("data", new AttributeValue { S = body });
                var putItemRequest = new PutItemRequest(TABLE_NAME, item);
                var putItemResponse = await client.PutItemAsync(putItemRequest);
                return putItemResponse;
            }
        }

        async public Task<object> DeleteItemAsync(string pk, string sk)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var key = GetKey(pk, sk);
                var deleteItemRequest = new DeleteItemRequest(TABLE_NAME, key);
                var deleteItemResponse = await client.DeleteItemAsync(deleteItemRequest);
                return deleteItemResponse;
            }
        }

    }
}
