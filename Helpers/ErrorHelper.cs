namespace RosterApiLambda.Helpers
{
    public static class ErrorHelper
    {
        public static string InvalidHttpMethodForResource(string httpMethod, string resource) => $"ERROR:  Unexpected 'httpMethod' value of '{httpMethod}' for resource '{resource}'.";
        public static string InvalidResource(string resource) => $"ERROR:  Unexpected resource of '{resource}'.";
        public static string InvalidReportId(string reportId) => $"ERROR:  Unexpected reportId of '{reportId}'.";
    }
}
