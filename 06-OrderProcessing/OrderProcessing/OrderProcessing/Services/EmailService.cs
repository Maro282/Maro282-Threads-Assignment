
using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using System.Xml.Linq;

namespace OrderProcessing.Services;

public class EmailService : IEmailService
{

    private readonly string Name = nameof(EmailService);
    public async Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct)
    {
        Log.Info(Name, $"Order #{order.Id} -> Rendering template...");
        await Task.Delay(200, ct);

        Log.Info(Name, $"Order #{order.Id} -> Connecting to SMTP...");
        await Task.Delay(300, ct);

        Log.Info(Name, $"Order #{order.Id} -> Sending...");
        await Task.Delay(500, ct);

        // Write to emails.json

        await JsonFileStore.UpdateJsonFileAsync<EmailMessage, bool>(DataFiles.emails, emails =>
        {
            emails.Add(new EmailMessage
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                To = customer.Email,
                Subject = $"Order #{order.Id} confirmation",
                Body = $"Hi {customer.Name}, your order #{order.Id} is confirmed. Total: {order.TotalAmount}",
                SentAt = DateTime.Now,
                Attempts = 1,
                LastError = null
            });
            return true;

        }, ct);
    }
}
