using System.ComponentModel.DataAnnotations;

namespace PayTech.BackOffice.Services.Strike.StrikeModels;

public class CreateInvoiceRequest
{
    [Required] public string CorrelationId { get; set; }

    public string Description { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [Required] public string Currency { get; set; }
}