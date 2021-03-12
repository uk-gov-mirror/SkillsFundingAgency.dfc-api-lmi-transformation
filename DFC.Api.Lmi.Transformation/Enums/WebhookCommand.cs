namespace DFC.Api.Lmi.Transformation.Enums
{
    public enum WebhookCommand
    {
        None,
        SubscriptionValidation,
        TransformSocToJobGroup,
        TransformAllSocToJobGroup,
        PurgeJobGroup,
        PurgeAllJobGroups,
    }
}
