using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using System.Transactions;

namespace OrderProcessing.Services;

public class PaymentGateway : IPaymentGateway
{
    private readonly string Name = nameof(PaymentGateway);
    public async Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct)
    {
        Log.Info(Name, $"Order #{orderId} -> Authorizing...");
        await Task.Delay(800, ct);

        Log.Info(Name, $"Order #{orderId} -> Fraud check...");
        await Task.Delay(500, ct);

        Log.Info(Name, $"Order #{orderId} -> Capturing...");
        await Task.Delay(700, ct);

        // Creating transaction fileName
        string transactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();

        //write in file
        await JsonFileStore.UpdateJsonFileAsync<PaymentTransaction, bool>(DataFiles.payments, payments =>
        {
            payments.Add(new PaymentTransaction
            {
                TransactionId = transactionId,
                Amount = amount,
                OrderId = orderId,
                Status = PaymentStatus.Captured,
                CreatedAt = DateTime.Now
            });
            return true;

        }, ct);

        return new PaymentResult
        {
            Success = true,
            TransactionId = transactionId,
            PaymentStatus = PaymentStatus.Captured
        };


    }



    public async Task RefundAsync(string transactionId, CancellationToken ct)
    {

        Log.Info(Name, $"Refund {transactionId} -> DNS/handshake...");
        await Task.Delay(150, ct);

        Log.Info(Name, $"Refund {transactionId} -> Sending request...");
        await Task.Delay(300, ct);

        Log.Info(Name, $"Refund {transactionId} -> Waiting for response...");
        await Task.Delay(Random.Shared.Next(500, 1501), ct);


        // Make transaction status refunded
        await JsonFileStore.UpdateJsonFileAsync<PaymentTransaction, bool>(DataFiles.payments, payments =>
        {

            PaymentTransaction? transaction = payments.FirstOrDefault(t => string.Equals(t.TransactionId, transactionId));
            if (transaction is null) throw new InvalidOperationException($"Transaction {transactionId} not found");


            transaction.Status = PaymentStatus.Refunded;
            return true;

        }, ct);

        Log.Info(Name, $"Refund {transactionId} -> done");

    }


}