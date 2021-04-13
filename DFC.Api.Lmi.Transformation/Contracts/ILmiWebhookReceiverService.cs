using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface ILmiWebhookReceiverService
    {
        WebhookRequestModel ExtractEvent(string requestBody);
    }
}
