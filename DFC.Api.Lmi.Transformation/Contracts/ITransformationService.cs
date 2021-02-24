using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface ITransformationService
    {
        Task GetAndTransformAsync();
    }
}
