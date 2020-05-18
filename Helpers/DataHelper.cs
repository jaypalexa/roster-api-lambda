using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace RosterApiLambda.Helpers
{
    public class DataHelper
    {
        private string TABLE_NAME { get; }

        public DataHelper(string tableName)
        {
            TABLE_NAME = $"roster-table-{tableName}";
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

        async public Task<List<T>> QueryAsync<T>(string pk, string sk)
        {
            var data = new List<T>();

            using var client = new AmazonDynamoDBClient();
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
                            var dataValue = JsonSerializer.Deserialize<T>(keyValuePair.Value.S);
                            data.Add(dataValue);
                        }
                    }
                }

                // Update marker variable
                startKey = queryResponse.LastEvaluatedKey;
            } while (startKey != null && startKey.Count > 0);

            return data;
        }

        async public Task<List<string>> QueryAsync(string pk, string sk)
        {
            var data = new List<string>();

            using var client = new AmazonDynamoDBClient();
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

        async public Task<T> GetItemAsync<T>(string pk, string sk)
        {
            using var client = new AmazonDynamoDBClient();
            var key = GetKey(pk, sk);
            var getItemRequest = new GetItemRequest(TABLE_NAME, key);
            var getItemResponse = await client.GetItemAsync(getItemRequest);
            var item = JsonSerializer.Deserialize<T>(getItemResponse.Item["data"].S);
            return item;
        }

        async public Task<string> GetItemAsync(string pk, string sk)
        {
            using var client = new AmazonDynamoDBClient();
            var key = GetKey(pk, sk);
            var getItemRequest = new GetItemRequest(TABLE_NAME, key);
            var getItemResponse = await client.GetItemAsync(getItemRequest);
            return getItemResponse.Item["data"].S;
        }

        async public Task<PutItemResponse> PutItemAsync<T>(string pk, string sk, T body)
        {
            using var client = new AmazonDynamoDBClient();
            var key = GetKey(pk, sk);
            var item = key.ToDictionary(x => x.Key, x => x.Value);
            item.Add("data", new AttributeValue { S = JsonSerializer.Serialize(body) });
            var putItemRequest = new PutItemRequest(TABLE_NAME, item);
            var putItemResponse = await client.PutItemAsync(putItemRequest);
            return putItemResponse;
        }

        async public Task<DeleteItemResponse> DeleteItemAsync(string pk, string sk)
        {
            using var client = new AmazonDynamoDBClient();
            var key = GetKey(pk, sk);
            var deleteItemRequest = new DeleteItemRequest(TABLE_NAME, key);
            var deleteItemResponse = await client.DeleteItemAsync(deleteItemRequest);
            return deleteItemResponse;
        }

    }
}
