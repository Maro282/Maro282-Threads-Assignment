
using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using System.Text.Json;

namespace OrderProcessing.Services;



public class EmailRetryRecord
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int Attempts { get; set; }
    public string LastError { get; set; } = "";
}


public class EmailRetryService
{
    private readonly IEmailService _emailService;

    private static readonly SemaphoreSlim FileLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly int[] RetryDelays =
    [
        500,
        1000,
        2000
    ];

    public EmailRetryService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendConfirmationWithRetryAsync(
        Order order,
        Customer customer,
        CancellationToken ct = default)
    {
        string lastError = "";

        for (int attempt = 1; attempt <= 4; attempt++)
        {
            try
            {
                await _emailService.SendConfirmationAsync(
                    order, customer, ct);

                Console.WriteLine(
                    $"Order #{order.Id}: Email sent successfully " +
                    $"on attempt {attempt}.");

                return;
            }
            catch (Exception ex) when (
                ex is not OperationCanceledException)
            {
                lastError = ex.Message;

                Console.WriteLine(
                    $"Order #{order.Id}: Email attempt " +
                    $"{attempt}/4 failed: {ex.Message}");

                if (attempt < 4)
                {
                    await Task.Delay(
                        RetryDelays[attempt - 1], ct);
                }
            }
        }

        // All 4 attempts failed.
        await SaveForRetryAsync(
            new EmailRetryRecord
            {
                OrderId = order.Id,
                CustomerId = customer.Id,
                Attempts = 4,
                LastError = lastError
            });

        Console.WriteLine(
            $"Email for Order #{order.Id} queued for retry later.");
    }

    private static async Task SaveForRetryAsync(
        EmailRetryRecord record)
    {
        await FileLock.WaitAsync();

        try
        {
            string directory = Path.Combine("Data");

            Directory.CreateDirectory(directory);

            string path = Path.Combine(
                directory, "email_retry.json");

            List<EmailRetryRecord> records = [];

            if (File.Exists(path))
            {
                string existing =
                    await File.ReadAllTextAsync(path);

                if (!string.IsNullOrWhiteSpace(existing))
                {
                    records =
                        JsonSerializer.Deserialize<
                            List<EmailRetryRecord>>(existing)
                        ?? [];
                }
            }

            records.Add(record);

            string json =
                JsonSerializer.Serialize(records, JsonOptions);

            await File.WriteAllTextAsync(path, json);
        }
        finally
        {
            FileLock.Release();
        }
    }
}
