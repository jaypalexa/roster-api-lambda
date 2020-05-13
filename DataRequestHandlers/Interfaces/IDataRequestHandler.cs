using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;

namespace RosterApiLambda.DataRequestHandlers.Interfaces
{
    public interface IDataRequestHandler
    {
        public static Task<object> Handle(string organizationId, RosterRequest request) => throw new NotImplementedException();
    }
}
