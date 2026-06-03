using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TLearn.Infrastructure.Services.PayOs;

public class PayOSService : IPayOSService

{
    private readonly HttpClient _httpClient;

    private readonly PayOSOptions _options;

    public PayOSService(
        HttpClient httpClient,
        IOptions<PayOSOptions> options)

    {
        _httpClient = httpClient;

        _options = options.Value;
    }

    public async Task<CreatePayOSPaymentResult> CreatePaymentLinkAsync(
        CreatePayOSPaymentRequest request,
        CancellationToken cancellationToken)

    {
        ValidateOptions();

        var endpoint = new Uri(
            new Uri(_options.BaseUrl),
            "/v2/payment-requests");

        var payload = new

        {
            orderCode = request.OrderCode,

            amount = request.Amount,

            description = request.Description,

            items = request.Items.Select(x => new

            {
                name = x.Name,

                quantity = x.Quantity,

                price = x.Price
            }).ToList(),

            cancelUrl = request.CancelUrl,

            returnUrl = request.ReturnUrl,

            signature = CreatePaymentRequestSignature(request)
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint);

        httpRequest.Headers.Add("x-client-id", _options.ClientId);

        httpRequest.Headers.Add("x-api-key", _options.ApiKey);

        httpRequest.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)

        {
            throw new InvalidOperationException(
                $"payOS tạo link thanh toán thất bại. StatusCode: {(int)response.StatusCode}, Body: {responseContent}");
        }

        var apiResponse = JsonSerializer.Deserialize<PayOSApiResponse<PayOSCreatePaymentData>>(
            responseContent,
            new JsonSerializerOptions

            {
                PropertyNameCaseInsensitive = true
            });

        if (apiResponse == null)

        {
            throw new InvalidOperationException(
                "payOS trả về dữ liệu không hợp lệ.");
        }

        if (!string.Equals(apiResponse.Code, "00", StringComparison.OrdinalIgnoreCase))

        {
            throw new InvalidOperationException(
                $"payOS tạo link thanh toán thất bại: {apiResponse.Desc}");
        }

        if (apiResponse.Data == null)

        {
            throw new InvalidOperationException(
                "payOS không trả về dữ liệu link thanh toán.");
        }

        return new CreatePayOSPaymentResult

        {
            PaymentLinkId = apiResponse.Data.PaymentLinkId,

            CheckoutUrl = apiResponse.Data.CheckoutUrl,

            QrCode = apiResponse.Data.QrCode,

            Status = apiResponse.Data.Status,

            OrderCode = apiResponse.Data.OrderCode,

            Amount = apiResponse.Data.Amount
        };
    }

    public PayOSWebhookData VerifyWebhook(string rawBody)
    {
        ValidateOptions();

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            throw new InvalidOperationException("Webhook body rỗng.");
        }

        var payload = JsonSerializer.Deserialize<PayOSWebhookPayload>(
            rawBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (payload == null)
        {
            throw new InvalidOperationException("Webhook payload không hợp lệ.");
        }

        if (payload.Data == null)
        {
            throw new InvalidOperationException("Webhook không có dữ liệu thanh toán.");
        }

        if (string.IsNullOrWhiteSpace(payload.Signature))
        {
            throw new InvalidOperationException("Webhook không có signature.");
        }

        var expectedSignature = CreateWebhookSignature(payload.Data);

        if (!string.Equals(
                expectedSignature,
                payload.Signature,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Webhook signature không hợp lệ.");
        }

        return payload.Data;
    }

    public async Task<PayOSPaymentInfo?> GetPaymentInfoAsync(
        string orderCode,
        CancellationToken cancellationToken)
    {
        ValidateOptions();

        if (string.IsNullOrWhiteSpace(orderCode))
        {
            throw new InvalidOperationException("OrderCode không hợp lệ.");
        }

        var endpoint = new Uri(
            new Uri(_options.BaseUrl),
            $"/v2/payment-requests/{orderCode}");

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            endpoint);

        httpRequest.Headers.Add("x-client-id", _options.ClientId);
        httpRequest.Headers.Add("x-api-key", _options.ApiKey);

        var response = await _httpClient.SendAsync(
            httpRequest,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"payOS kiểm tra đơn hàng thất bại. StatusCode: {(int)response.StatusCode}, Body: {responseContent}");
        }

        var apiResponse = JsonSerializer.Deserialize<PayOSApiResponse<PayOSPaymentInfo>>(
            responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (apiResponse == null)
        {
            throw new InvalidOperationException(
                "payOS trả về dữ liệu kiểm tra thanh toán không hợp lệ.");
        }

        if (!string.Equals(apiResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"payOS kiểm tra thanh toán thất bại: {apiResponse.Desc}");
        }

        return apiResponse.Data;
    }

    private string CreateWebhookSignature(PayOSWebhookData data)
    {
        var rawData =
            $"amount={data.Amount}" +
            $"&code={data.Code}" +
            $"&desc={data.Desc}" +
            $"&orderCode={data.OrderCode}";

        return HmacSha256(rawData, _options.ChecksumKey);
    }

    private string CreatePaymentRequestSignature(
        CreatePayOSPaymentRequest request)

    {
        var rawData =
            $"amount={request.Amount}" +
            $"&cancelUrl={request.CancelUrl}" +
            $"&description={request.Description}" +
            $"&orderCode={request.OrderCode}" +
            $"&returnUrl={request.ReturnUrl}";

        return HmacSha256(rawData, _options.ChecksumKey);
    }

    private static string HmacSha256(
        string data,
        string key)

    {
        var keyBytes = Encoding.UTF8.GetBytes(key);

        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);

        var hashBytes = hmac.ComputeHash(dataBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }

    private void ValidateOptions()

    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))

        {
            throw new InvalidOperationException("Chưa cấu hình PayOS:ClientId.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))

        {
            throw new InvalidOperationException("Chưa cấu hình PayOS:ApiKey.");
        }

        if (string.IsNullOrWhiteSpace(_options.ChecksumKey))

        {
            throw new InvalidOperationException("Chưa cấu hình PayOS:ChecksumKey.");
        }

        if (string.IsNullOrWhiteSpace(_options.ReturnUrl))

        {
            throw new InvalidOperationException("Chưa cấu hình PayOS:ReturnUrl.");
        }

        if (string.IsNullOrWhiteSpace(_options.CancelUrl))

        {
            throw new InvalidOperationException("Chưa cấu hình PayOS:CancelUrl.");
        }
    }
}