using System.ComponentModel.DataAnnotations;
using BitPay;
using BitPay.Models;
using BitPay.Models.Invoice;
using BitPay.Models.Rate;
using Microsoft.Extensions.Configuration;
using PayTech.BackOffice.Data.Authentication;
using Environment = BitPay.Environment;
using Invoice = BitPay.Models.Invoice.Invoice;

namespace PayTech.BackOffice.Services.Bitpay;

public class BitPayClient
{
    private readonly Client _bitpay;
    private readonly IConfiguration _configuration;

    public BitPayClient(IConfiguration configuration, Environment environment)
    {
        _configuration = configuration;
        var configFilePath = _configuration.GetSection("ConfigFilePath").Value;
        if (environment == Environment.Test) configFilePath = configFilePath!.Replace("Prod", "Test");
        _bitpay = new Client(new ConfigFilePath(configFilePath!), environment);
    }

    /// <summary>
    ///     Create an invoice using the specified facade.
    ///     https://developer.bitpay.com/docs/create-invoice#redirecturl
    ///     https://developer.bitpay.com/reference/c-full-sdk-create-an-invoice
    /// </summary>
    /// <param name="invoiceRequest">An invoice request object.</param>
    /// <returns>A new invoice object returned from the server.</returns>
    /// <throws>InvoiceCreationException InvoiceCreationException class</throws>
    /// <throws>BitPayException BitPayException class</throws>
    public Task<Invoice> CreateInvoice(CreateBitpayInvoiceRequest invoiceRequest, ApplicationUser profile)
    {
        var dashboardUrl = _configuration["RedirectUrl"]!;

        var invoice = new Invoice(invoiceRequest.Amount, Currency.USD)
        {
            OrderId = invoiceRequest.OrderId,
            ItemDesc = invoiceRequest.Description,
            NotificationEmail = "systems@paytech.systems",

            FullNotifications = true,
            ExtendedNotifications =
                true, // https://developer.bitpay.com/reference/notifications-invoices#extended-ipn-format
            TransactionSpeed = "fast",
            NotificationUrl =
                "https://api-test.test.paytech.systems/api/BitPayWebhook", // TODO: Change to the correct URL
            AutoRedirect = true,
            RedirectUrl = dashboardUrl + "?action=bitpay_success",
            CloseURL = dashboardUrl + "?action=bitpay_close"
        };


        // TODO: Set the correct buyer information based on the context
        invoice.Buyer = new Buyer
        {
            Name = profile.FirstName,
            //Address1 = "SomeStreet",
            //Address2 = "911",
            //Locality = "Washington",
            //Region = "District of Columbia",
            //PostalCode = "20000",
            //Country = "USA",
            Email = profile.Email,
            Phone = profile.PhoneNumber,
            Notify = true
        };

        return _bitpay.CreateInvoice(invoice);
    }

    /// <summary>
    ///     Retrieve an invoice by id and token.
    /// </summary>
    /// <param name="invoiceId">The id of the requested invoice.</param>
    /// <returns>The invoice object retrieved from the server.</returns>
    /// <throws>InvoiceQueryException InvoiceQueryException class</throws>
    /// <throws>BitPayException BitPayException class</throws>
    public Task<Invoice> GetInvoice(string invoiceId)
    {
        return _bitpay.GetInvoice(invoiceId);
    }

    // TODO: Uncomment and implement if needed
    //public Task<Invoice> GetInvoiceByGuid(string invoiceGuid)
    //{
    //    return _bitpay.GetInvoiceByGuid(invoiceGuid);
    //}

    /// <summary>
    ///     Retrieve a list of invoices by date range using the merchant facade.
    /// </summary>
    /// <param name="startDate">The start date for the query.</param>
    /// <param name="endDate">The end date for the query.</param>
    /// <returns>A list of invoice objects retrieved from the server.</returns>
    /// <throws>InvoiceQueryException InvoiceQueryException class</throws>
    /// <throws>BitPayException BitPayException class</throws>
    public Task<List<Invoice>> GetInvoices(DateTime startDate, DateTime endDate)
    {
        return _bitpay.GetInvoices(startDate, endDate);
    }

    /// <summary>
    ///     Retrieves a bus token which can be used to subscribe to invoice events.
    /// </summary>
    /// <param name="invoiceId">the id of the invoice for which you want to fetch an event token</param>
    /// <returns>event token</returns>
    /// <exception cref="BitPayException"></exception>
    public Task<InvoiceEventToken> GetInvoiceEventToken(string invoiceId)
    {
        return _bitpay.GetInvoiceEventToken(invoiceId);
    }

    /// <summary>
    ///     Update a BitPay invoice.
    /// </summary>
    /// <param name="invoiceId">The id of the invoice to updated.</param>
    /// <param name="parameters">Available parameters: buyerEmail, buyerSms, smsCode, autoVerify</param>
    /// <returns>A BitPay updated Invoice object.</returns>
    /// <throws>InvoiceUpdateException InvoiceUpdateException class</throws>
    public Task<Invoice> UpdateInvoice(string invoiceId, Dictionary<string, dynamic?> parameters)
    {
        return _bitpay.UpdateInvoice(invoiceId, parameters);
    }

    /// <summary>
    ///     Cancel a BitPay invoice.
    /// </summary>
    /// <param name="invoiceId">The id of the invoice to cancel.</param>
    /// <returns>Cancelled invoice object.</returns>
    /// <throws>InvoiceCancellationException InvoiceCancellationException class</throws>
    public Task<Invoice> CancelInvoice(string invoiceId)
    {
        return _bitpay.CancelInvoice(invoiceId);
    }

    /// <summary>
    ///     Cancel a BitPay invoice.
    /// </summary>
    /// <param name="invoiceGuid">The GUID of the invoice to cancel.</param>
    /// <returns>Cancelled invoice object.</returns>
    /// <throws>InvoiceCancellationException InvoiceCancellationException class</throws>
    public Task<Invoice> CancelInvoiceByGuid(string invoiceGuid)
    {
        return _bitpay.CancelInvoiceByGuid(invoiceGuid);
    }

    /// <summary>
    ///     Retrieve the exchange rate table using the public facade.
    /// </summary>
    /// <returns>The rate table as an object retrieved from the server.</returns>
    /// <throws>RatesQueryException RatesQueryException class</throws>
    /// <throws>BitPayException BitPayException class</throws>
    public async Task<Rates> GetExchangeRatesAsync()
    {
        var rates = await _bitpay.GetRates();
        return rates;
    }
}

public class CreateBitpayInvoiceRequest
{
    [Required] public decimal Amount { get; set; }
    public string OrderId { get; set; }
    public string Description { get; set; }
    public string NotificationEmail { get; set; }
}