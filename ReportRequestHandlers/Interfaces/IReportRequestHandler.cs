using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;

namespace RosterApiLambda.ReportRequestHandlers.Interfaces
{
    public interface IReportRequestHandler
    {
        public static Task<object> Handle(string organizationId, string reportId, RosterRequest request) => throw new NotImplementedException();
    }
}
