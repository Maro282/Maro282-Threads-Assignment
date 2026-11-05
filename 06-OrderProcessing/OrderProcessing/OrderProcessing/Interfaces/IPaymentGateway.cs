using OrderProcessing.Models;

namespace OrderProcessing.Interfaces;

public interface IPaymentGateway
{
    public Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct);
    public Task RefundAsync(string transactionId, CancellationToken ct);
}






