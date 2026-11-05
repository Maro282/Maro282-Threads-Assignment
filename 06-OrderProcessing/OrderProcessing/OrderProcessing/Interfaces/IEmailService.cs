

using OrderProcessing.Models;

namespace OrderProcessing.Interfaces;

public interface IEmailService
{

    public Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct = default);
}
