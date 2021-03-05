using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface ITransformationService
    {
        Task<HttpStatusCode> TransformAsync();

        Task<HttpStatusCode> TransformItemAsync(Uri url);

        Task<bool> PurgeAsync();

        Task<bool> DeleteAsync(Guid contentId);
    }
}
