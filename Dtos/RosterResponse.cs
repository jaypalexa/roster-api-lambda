namespace RosterApiLambda.Dtos
{
    /* AWS LAMBDA INTEGRATION RESPONSE FORMAT
      {
        "cookies" : ["cookie1", "cookie2"]
        "isBase64Encoded": true|false,
        "statusCode": httpStatusCode,
        "headers": { "headerName": "headerValue", ... },
        "body": "Hello from Lambda!"
      }   
    */

    public class RosterResponse
    {
        public RosterResponse()
        {
            body = new RosterResponseBody();
        }

        public RosterResponseBody body { get; set; }
    }
}
