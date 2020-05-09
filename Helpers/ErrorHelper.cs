namespace RosterApiLambda.Helpers
{
    public class ErrorHelper
    {
        public static string InvalidHttpMethodForResource(string httpMethod, string resource) => $"ERROR:  Unexpected 'httpMethod' value of '{httpMethod}' for resource '{resource}'.";
        public static string InvalidResource(string resource) => $"ERROR:  Unexpected resource of '{resource}'.";
    }
}
