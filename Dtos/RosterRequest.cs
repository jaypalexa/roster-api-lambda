﻿using System.Collections.Generic;
using System.Text.Json;

namespace RosterApiLambda.Dtos
{
    public class RosterRequest
    {
        public RosterRequest()
        {
            headers = new Dictionary<string, string>();
            queryStringParameters = new Dictionary<string, string>();
            pathParameters = new Dictionary<string, string>();
        }

        public string resource { get; set; }                // ??? /root/child
        public string httpMethod { get; set; }              // DELETE|GET|HEAD|OPTIONS|PATCH|POST|PUT
        public Dictionary<string, string> headers { get; set; }                // { "headerName": "headerValue", ... }
        public Dictionary<string, string> queryStringParameters { get; set; }  // { "key": "value", ... }
        public Dictionary<string, string> pathParameters { get; set; }         // { "key": "value", ... }
        public JsonElement body { get; set; }               // JSON
        public bool isBase64Encoded { get; set; }           // true|false
    };
}
