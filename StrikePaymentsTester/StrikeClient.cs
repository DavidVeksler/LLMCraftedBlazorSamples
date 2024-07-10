using Newtonsoft.Json;
using PayTech.BackOffice.Services.Strike.StrikeModels;
using RestSharp;
using Serilog;

public class StrikeClient
{
    private readonly string _apiKey;
    private readonly RestClient _client;
    private readonly ILogger _logger;

    public StrikeClient(string apiKey, string baseUrl = "https://api.strike.me/v1/")
    {
        _apiKey = apiKey;
        _client = new RestClient(baseUrl);
        _logger = Log.ForContext<StrikeClient>();
    }

    // <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="correlationId">Unique identifier for the invoice (max 40 characters).</param>
    /// <param name="description">Invoice description (max 200 characters).</param>
    /// <param name="amount">Invoice amount.</param>
    /// <param name="currency">Invoice currency (BTC, USD, EUR, or USDT).</param>
    /// <returns>The created invoice.</returns>
    /// <exception cref="BadRequestException">Thrown when the request is invalid.</exception>
    /// <exception cref="UnauthorizedException">Thrown when the API key is invalid or missing.</exception>
    /// <exception cref="ForbiddenException">Thrown when the API key doesn't have sufficient permissions.</exception>
    /// <exception cref="NotFoundException">Thrown when the requested resource is not found.</exception>
    /// <exception cref="ConflictException">Thrown when there's a conflict with the current state of the resource.</exception>
    /// <exception cref="UnprocessableEntityException">Thrown when the request is unprocessable.</exception>
    /// <exception cref="TooManyRequestsException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when there's an internal server error.</exception>
    /// <exception cref="ServiceUnavailableException">Thrown when the service is unavailable.</exception>
    public async Task<Invoice> CreateInvoice(string correlationId, string description, decimal amount, string currency)
    {
        var request = new RestRequest("invoices", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new
        {
            correlationId,
            description,
            amount = new
            {
                amount,
                currency
            }
        });

        _logger.Information("Creating invoice with correlation ID: {CorrelationId}", correlationId);

        var response = await _client.ExecuteAsync<Invoice>(request);
        HandleErrors(response);

        _logger.Information("Invoice created successfully. Invoice ID: {InvoiceId}", response.Data.InvoiceId);

        return response.Data;
    }

    /// <summary>
    ///     Generates a quote for an invoice.
    ///     https://docs.strike.me/api/issue-quote-for-invoice
    /// </summary>
    /// <param name="invoiceId">The ID of the invoice.</param>
    /// <param name="descriptionHash">Optional description hash for the Lightning invoice.</param>
    /// <returns>The generated quote.</returns>
    /// <exception cref="BadRequestException">Thrown when the request is invalid.</exception>
    /// <exception cref="UnauthorizedException">Thrown when the API key is invalid or missing.</exception>
    /// <exception cref="ForbiddenException">Thrown when the API key doesn't have sufficient permissions.</exception>
    /// <exception cref="NotFoundException">Thrown when the requested resource is not found.</exception>
    /// <exception cref="ConflictException">Thrown when there's a conflict with the current state of the resource.</exception>
    /// <exception cref="UnprocessableEntityException">Thrown when the request is unprocessable.</exception>
    /// <exception cref="TooManyRequestsException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when there's an internal server error.</exception>
    /// <exception cref="ServiceUnavailableException">Thrown when the service is unavailable.</exception>
    public async Task<Quote> GenerateQuote(string invoiceId, string descriptionHash = null)
    {
        var request = new RestRequest($"invoices/{invoiceId}/quote", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(descriptionHash)) request.AddJsonBody(new { descriptionHash });


        _logger.Information("Generating quote for invoice ID: {InvoiceId}", invoiceId);

        var response = await _client.ExecuteAsync<Quote>(request);
        HandleErrors(response);

        _logger.Information("Quote generated successfully. Quote ID: {QuoteId}", response.Data.QuoteId);

        return response.Data;
    }


    /// <summary>
    ///     Retrieves an invoice by its ID.
    /// </summary>
    /// <param name="invoiceId">The ID of the invoice.</param>
    /// <returns>The retrieved invoice.</returns>
    /// <exception cref="BadRequestException">Thrown when the request is invalid.</exception>
    /// <exception cref="UnauthorizedException">Thrown when the API key is invalid or missing.</exception>
    /// <exception cref="ForbiddenException">Thrown when the API key doesn't have sufficient permissions.</exception>
    /// <exception cref="NotFoundException">Thrown when the requested resource is not found.</exception>
    /// <exception cref="TooManyRequestsException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when there's an internal server error.</exception>
    /// <exception cref="ServiceUnavailableException">Thrown when the service is unavailable.</exception>
    public async Task<Invoice> GetInvoice(string invoiceId)
    {
        var request = new RestRequest($"invoices/{invoiceId}");
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Accept", "application/json");

        _logger.Information("Retrieving invoice with ID: {InvoiceId}", invoiceId);

        var response = await _client.ExecuteAsync<Invoice>(request);
        HandleErrors(response);

        _logger.Information("Invoice retrieved successfully. Invoice ID: {InvoiceId}", response.Data.InvoiceId);

        return response.Data;
    }

    private void HandleErrors(RestResponse response)
    {
        if (response.IsSuccessful)
            return;

        _logger.Error("Strike API request failed with status code: {StatusCode}. Error message: {ErrorMessage}",
            response.StatusCode, response.ErrorMessage);
        throw new Exception($"Error: {response.Content}");
    }

    private void HandleErrors<T>(RestResponse<T> response)
    {
        if (response.IsSuccessful)
            return;

        _logger.Error("Strike API request failed with status code: {StatusCode}. Error message: {ErrorMessage}",
            response.StatusCode, response.ErrorMessage);

        if (response.Content != null)
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
                var errorMessage = $"Error Code: {errorResponse.Data.Code}, Message: {errorResponse.Data.Message}";

                if (errorResponse.Data.ValidationErrors != null)
                {
                    var validationErrors = string.Join(", ",
                        errorResponse.Data.ValidationErrors.SelectMany(
                            x => x.Value.Select(y => $"{x.Key}: {y.Message}")));
                    errorMessage += $", Validation Errors: {validationErrors}";
                }

                throw new Exception(errorMessage);
            }
            catch (JsonException)
            {
                throw new Exception($"Error: {response.Content}");
            }

        throw new Exception($"Error: {response.ErrorMessage}");
    }

    public async Task CreateWebhookSubscription(string webhookUrl, string secret)
    {
        var request = new RestRequest("subscriptions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new
        {
            webhookUrl,
            webhookVersion = "v1",
            secret,
            enabled = true,
            eventTypes = new[] { "invoice.created", "invoice.updated" }
        });

        var response = await _client.ExecuteAsync(request);
        // Handle the response and any errors
    }

    /// <summary>
    ///     Cancels an invoice.
    /// </summary>
    /// <param name="invoiceId">The ID of the invoice to cancel.</param>
    /// <returns>The canceled invoice.</returns>
    /// <exception cref="BadRequestException">Thrown when the request is invalid.</exception>
    /// <exception cref="UnauthorizedException">Thrown when the API key is invalid or missing.</exception>
    /// <exception cref="ForbiddenException">Thrown when the API key doesn't have sufficient permissions.</exception>
    /// <exception cref="NotFoundException">Thrown when the requested resource is not found.</exception>
    /// <exception cref="ConflictException">Thrown when there's a conflict with the current state of the resource.</exception>
    /// <exception cref="UnprocessableEntityException">Thrown when the request is unprocessable.</exception>
    /// <exception cref="TooManyRequestsException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="InternalServerErrorException">Thrown when there's an internal server error.</exception>
    /// <exception cref="ServiceUnavailableException">Thrown when the service is unavailable.</exception>
    public async Task<Invoice> CancelInvoice(string invoiceId)
    {
        var request = new RestRequest($"invoices/{invoiceId}/cancel", Method.Patch);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");

        _logger.Information("Canceling invoice with ID: {InvoiceId}", invoiceId);

        var response = await _client.ExecuteAsync<Invoice>(request);
        HandleErrors(response);

        _logger.Information("Invoice canceled successfully. Invoice ID: {InvoiceId}", response.Data.InvoiceId);

        return response.Data;
    }

    public async Task<InvoiceResponse> GetInvoices(string filter = null, string orderBy = null, int? skip = null,
        int? top = null)
    {
        var request = new RestRequest("invoices");
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(filter)) request.AddQueryParameter("$filter", filter);

        if (!string.IsNullOrEmpty(orderBy)) request.AddQueryParameter("$orderby", orderBy);

        if (skip.HasValue) request.AddQueryParameter("$skip", skip.Value.ToString());

        if (top.HasValue) request.AddQueryParameter("$top", top.Value.ToString());

        var response = await _client.ExecuteAsync(request);
        HandleErrors(response);
        var invoiceResponse = JsonConvert.DeserializeObject<InvoiceResponse>(response.Content);
        return invoiceResponse;
    }


    public async Task<AccountBalanceDetails> GetAccountBalanceDetails()
    {
        var request = new RestRequest("account/balance");
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Accept", "application/json");

        _logger.Information("Retrieving account balance details.");

        var response = await _client.ExecuteAsync<AccountBalanceDetails>(request);
        HandleErrors(response);

        _logger.Information("Account balance details retrieved successfully.");

        return response.Data;
    }


    public async Task<CurrencyExchangeQuote> CreateCurrencyExchangeQuote(string fromCurrency, string toCurrency,
        decimal amount)
    {
        var request = new RestRequest("exchange/quote", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new
        {
            fromCurrency,
            toCurrency,
            amount
        });

        _logger.Information(
            "Creating currency exchange quote from {FromCurrency} to {ToCurrency} for amount {Amount}",
            fromCurrency, toCurrency, amount);

        var response = await _client.ExecuteAsync<CurrencyExchangeQuote>(request);
        HandleErrors(response);

        _logger.Information("Currency exchange quote created successfully. Quote ID: {QuoteId}",
            response.Data.QuoteId);

        return response.Data;
    }


    public async Task<CurrencyExchangeQuote> ExecuteCurrencyExchangeQuote(string quoteId)
    {
        var request = new RestRequest($"exchange/quote/{quoteId}/execute", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");

        _logger.Information("Executing currency exchange quote with ID: {QuoteId}", quoteId);

        var response = await _client.ExecuteAsync<CurrencyExchangeQuote>(request);
        HandleErrors(response);

        _logger.Information("Currency exchange quote executed successfully. Quote ID: {QuoteId}",
            response.Data.QuoteId);

        return response.Data;
    }


    public async Task<Payout> CreatePayout(string currency, decimal amount, string recipient)
    {
        var request = new RestRequest("payouts", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new
        {
            currency,
            amount,
            recipient
        });

        _logger.Information("Creating payout in {Currency} for amount {Amount} to recipient {Recipient}",
            currency, amount, recipient);

        var response = await _client.ExecuteAsync<Payout>(request);
        HandleErrors(response);

        _logger.Information("Payout created successfully. Payout ID: {PayoutId}", response.Data.PayoutId);

        return response.Data;
    }


    public async Task<Payout> InitiatePayout(string payoutId)
    {
        var request = new RestRequest($"payouts/{payoutId}/initiate", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");

        _logger.Information("Initiating payout with ID: {PayoutId}", payoutId);

        var response = await _client.ExecuteAsync<Payout>(request);
        HandleErrors(response);

        _logger.Information("Payout initiated successfully. Payout ID: {PayoutId}",
            response.Data.PayoutId);

        return response.Data;
    }


    public async Task<List<CurrencyExchangeRateTicker>> GetCurrencyExchangeRateTickers()
    {
        var request = new RestRequest("exchange/rates/ticker");
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Accept", "application/json");

        _logger.Information("Retrieving currency exchange rate tickers.");

        var response = await _client.ExecuteAsync<List<CurrencyExchangeRateTicker>>(request);
        HandleErrors(response);

        _logger.Information("Currency exchange rate tickers retrieved successfully.");

        return response.Data;
    }


    public async Task<Subscription> CreateSubscription(string @event, string callbackUrl)
    {
        var request = new RestRequest("subscriptions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new
        {
            @event,
            callbackUrl
        });

        _logger.Information(
            "Creating subscription for event {Event} with callback URL {CallbackUrl}", @event,
            callbackUrl);

        var response = await _client.ExecuteAsync<Subscription>(request);
        HandleErrors(response);

        _logger.Information(
            "Subscription created successfully. Subscription ID: {SubscriptionId}",
            response.Data.SubscriptionId);

        return response.Data;
    }


    public async Task<List<Event>> GetEvents()
    {
        var request = new RestRequest("events");
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
        request.AddHeader("Accept", "application/json");

        _logger.Information("Retrieving events.");

        var response = await _client.ExecuteAsync<List<Event>>(request);
        HandleErrors(response);

        _logger.Information("Events retrieved successfully.");

        return response.Data;
    }

    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public ErrorData Data { get; set; }
    }

    public class ErrorData
    {
        public int Status { get; set; }
        public Dictionary<string, List<ValidationError>> ValidationErrors { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public class ValidationError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Values { get; set; }
    }

    public class InvoiceResponse
    {
        public List<Invoice> Items { get; set; }
        public int Count { get; set; }
    }


    // Model for Account Balance Details
    public class AccountBalanceDetails
    {
        public string Currency { get; set; }
        public decimal Balance { get; set; }
    }

    // Model for Currency Exchange Quote
    public class CurrencyExchangeQuote
    {
        public string QuoteId { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public DateTime Expiry { get; set; }
    }

    // Model for Payout
    public class Payout
    {
        public string PayoutId { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Model for Currency Exchange Rate Tickers
    public class CurrencyExchangeRateTicker
    {
        public string CurrencyPair { get; set; }
        public decimal Rate { get; set; }
    }

    // Model for Subscription
    public class Subscription
    {
        public string SubscriptionId { get; set; }
        public string Event { get; set; }
        public string CallbackUrl { get; set; }
    }

    // Model for Event
    public class Event
    {
        public string EventId { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
    }
}