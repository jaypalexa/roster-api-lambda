using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;

namespace RosterApiLambda.Handlers.Interfaces
{
    public interface IHandler
    {
        public static Task<object> Handle(string organizationId, RosterRequest request) => throw new NotImplementedException();
    }
}
