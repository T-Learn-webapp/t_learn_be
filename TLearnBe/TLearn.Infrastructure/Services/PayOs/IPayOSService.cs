namespace TLearn.Infrastructure.Services.PayOs;

public interface IPayOSService

{
    Task<CreatePayOSPaymentResult> CreatePaymentLinkAsync(
        CreatePayOSPaymentRequest request,
        CancellationToken cancellationToken);


    PayOSWebhookData VerifyWebhook(
        string rawBody);

    Task<PayOSPaymentInfo?> GetPaymentInfoAsync(
        string orderCode,
        CancellationToken cancellationToken);
}