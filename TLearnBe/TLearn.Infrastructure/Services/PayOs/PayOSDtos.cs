namespace TLearn.Infrastructure.Services.PayOs;

public class CreatePayOSPaymentRequest

{

    public long OrderCode { get; set; }

    public int Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    public string CancelUrl { get; set; } = string.Empty;

    public List<CreatePayOSPaymentItem> Items { get; set; } = new();

}

public class CreatePayOSPaymentItem

{

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public int Price { get; set; }

}

public class CreatePayOSPaymentResult

{

    public string? PaymentLinkId { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? QrCode { get; set; }

    public string? Status { get; set; }

    public long OrderCode { get; set; }

    public int Amount { get; set; }

}

public class PayOSApiResponse<T>

{

    public string Code { get; set; } = string.Empty;

    public string Desc { get; set; } = string.Empty;

    public T? Data { get; set; }

    public string? Signature { get; set; }

}

public class PayOSCreatePaymentData

{

    public string? Bin { get; set; }

    public string? AccountNumber { get; set; }

    public string? AccountName { get; set; }

    public int Amount { get; set; }

    public string? Description { get; set; }

    public long OrderCode { get; set; }

    public string? Currency { get; set; }

    public string? PaymentLinkId { get; set; }

    public string? Status { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? QrCode { get; set; }

}

public class PayOSWebhookPayload

{

    public string Code { get; set; } = string.Empty;

    public string Desc { get; set; } = string.Empty;

    public bool Success { get; set; }

    public PayOSWebhookData? Data { get; set; }

    public string Signature { get; set; } = string.Empty;

}

public class PayOSWebhookData

{

    public long OrderCode { get; set; }

    public int Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string TransactionDateTime { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string PaymentLinkId { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Desc { get; set; } = string.Empty;

}
public class PayOSPaymentInfo
{
    public string? Id { get; set; }

    public long OrderCode { get; set; }

    public int Amount { get; set; }

    public int AmountPaid { get; set; }

    public int AmountRemaining { get; set; }

    public string? Status { get; set; }

    public string? CreatedAt { get; set; }

    public List<PayOSTransactionInfo> Transactions { get; set; } = new();
}

public class PayOSTransactionInfo
{
    public string? Reference { get; set; }

    public int Amount { get; set; }

    public string? AccountNumber { get; set; }

    public string? Description { get; set; }

    public string? TransactionDateTime { get; set; }

    public string? VirtualAccountName { get; set; }

    public string? VirtualAccountNumber { get; set; }

    public string? CounterAccountBankId { get; set; }

    public string? CounterAccountBankName { get; set; }

    public string? CounterAccountName { get; set; }

    public string? CounterAccountNumber { get; set; }
}