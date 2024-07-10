namespace PayTech.BackOffice.Services.Strike.StrikeModels;

/// <summary>
///     Represents an error response from the Strike API.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    ///     Gets or sets the error data.
    /// </summary>
    public ErrorData Data { get; set; }
}

/// <summary>
///     Represents the error data in an error response.
/// </summary>
public class ErrorData
{
    /// <summary>
    ///     Gets or sets the error code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
///     Represents an invoice.
/// </summary>
public class Invoice
{
    /// <summary>
    ///     Gets or sets the unique identifier of the invoice.
    /// </summary>
    public string InvoiceId { get; set; }

    /// <summary>
    ///     Gets or sets the amount of the invoice.
    /// </summary>
    public Amount Amount { get; set; }

    /// <summary>
    ///     Gets or sets the state of the invoice.
    ///     Possible values: UNPAID, PENDING, PAID, CANCELLED.
    /// </summary>
    public string State { get; set; }

    /// <summary>
    ///     Gets or sets the creation date and time of the invoice.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    ///     Gets or sets the unique correlation ID of the invoice.
    ///     This can be used to correlate the invoice with an external entity.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    ///     Gets or sets the description of the invoice.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the invoice issuer.
    /// </summary>
    public string IssuerId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the invoice receiver.
    /// </summary>
    public string ReceiverId { get; set; }
}

/// <summary>
///     Represents an amount with a value and currency.
/// </summary>
public class Amount
{
    /// <summary>
    ///     Gets or sets the value of the amount.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     Gets or sets the currency of the amount.
    ///     Possible values: BTC, USD, EUR, USDT.
    /// </summary>
    public string Currency { get; set; }
}

/// <summary>
///     Represents a quote for an invoice.
/// </summary>
public class Quote
{
    /// <summary>
    ///     Gets or sets the unique identifier of the quote.
    /// </summary>
    public string QuoteId { get; set; }

    /// <summary>
    ///     Gets or sets the description of the quote.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Gets or sets the Lightning invoice associated with the quote.
    /// </summary>
    public string LnInvoice { get; set; }

    /// <summary>
    ///     Gets or sets the on-chain Bitcoin address associated with the quote.
    /// </summary>
    public string OnchainAddress { get; set; }

    /// <summary>
    ///     Gets or sets the expiration date and time of the quote.
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    ///     Gets or sets the expiration time of the quote in seconds.
    /// </summary>
    public int ExpirationInSec { get; set; }

    /// <summary>
    ///     Gets or sets the source amount of the quote.
    /// </summary>
    public Amount SourceAmount { get; set; }

    /// <summary>
    ///     Gets or sets the target amount of the quote.
    /// </summary>
    public Amount TargetAmount { get; set; }

    /// <summary>
    ///     Gets or sets the conversion rate between the source and target currencies.
    /// </summary>
    public ConversionRate ConversionRate { get; set; }
}

/// <summary>
///     Represents a conversion rate between two currencies.
/// </summary>
public class ConversionRate
{
    /// <summary>
    ///     Gets or sets the conversion rate amount.
    /// </summary>
    public string Amount { get; set; }

    /// <summary>
    ///     Gets or sets the source currency of the conversion rate.
    /// </summary>
    public string SourceCurrency { get; set; }

    /// <summary>
    ///     Gets or sets the target currency of the conversion rate.
    /// </summary>
    public string TargetCurrency { get; set; }
}