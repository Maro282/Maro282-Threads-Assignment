
namespace OrderProcessing;

public class PaymentResult
{

    public bool Success { get; set; }
    public string? TransactionId { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? Message { get; set; }
}
