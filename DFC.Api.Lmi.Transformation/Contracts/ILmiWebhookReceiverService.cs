using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface ILmiWebhookReceiverService
    {
        Task<IActionResult> ReceiveEventsAsync(string requestBody);
    }
}
