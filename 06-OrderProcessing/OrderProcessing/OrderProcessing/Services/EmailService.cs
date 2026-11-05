
using OrderProcessing.Exceptions;
using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using System.Collections.Concurrent;

namespace OrderProcessing.Services;

public class EmailService : IEmailService
{

    private readonly string Name = nameof(EmailService);

    private readonly IDictionary<int, int> _failAttempts;
    private readonly ConcurrentDictionary<int, int> _attempts;

    public EmailService(IDictionary<int, int>? failAttempts = null)
    {
        _failAttempts = failAttempts is null ? new Dictionary<int, int>() : new Dictionary<int, int>(failAttempts);
    }

    public async Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct)
    {

        int attempts = _attempts.AddOrUpdate(order.Id, 1, (_, current) => current + 1);

        Log.Info(Name, $"Order #{order.Id} -> Rendering template...");
        await Task.Delay(200, ct);

        Log.Info(Name, $"Order #{order.Id} -> Connecting to SMTP...");
        await Task.Delay(300, ct);

        // injected failure: the first N attempts of this order throw
        if (_failAttempts.TryGetValue(order.Id, out int failCount) && attempts <= failCount)
        {
            Log.Error(Name, $"Order #{order.Id} -> SMTP unavailable (attempt {attempts})");
            throw new SmtpUnavailableException("SMTP server unavailable");
        }


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
                Attempts = attempts, // updated it will record no of the succeeded attempt
                LastError = null
            });
            return true;

        }, ct);
    }
}
