using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using System.Transactions;

namespace OrderProcessing.Services;

public class PaymentGateway : IPaymentGateway
{
    private readonly string Name = nameof(PaymentGateway);
    private readonly HashSet<int> _timeoutOrderIds;
    private readonly HashSet<int> _declinedOrderIds;


    // Throw failure through constructors
    public PaymentGateway(IEnumerable<int>? timeoutOrderIds = null, IEnumerable<int>? declinedOrderIds = null)
    {
        _timeoutOrderIds = new HashSet<int>(timeoutOrderIds ?? Enumerable.Empty<int>());
        _declinedOrderIds = new HashSet<int>(declinedOrderIds ?? Enumerable.Empty<int>());
    }



    public async Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct)
    {
        Log.Info(Name, $"Order #{orderId} -> Authorizing...");
        await Task.Delay(800, ct);

        Log.Info(Name, $"Order #{orderId} -> Fraud check...");
        await Task.Delay(500, ct);

        if (_declinedOrderIds.Contains(orderId))
        {
            Log.Error(Name, $"Order #{orderId} -> Card declined by issuer");
            await AddRecordAsync(orderId, amount, PaymentStatus.Declined, null, ct);

            return new PaymentResult
            {
                Success = false,
                TransactionId = null,
                PaymentStatus = PaymentStatus.Declined,
                Message = "Card declined by issuer"
            };
        }

        Log.Info(Name, $"Order #{orderId} -> Capturing...");
        await Task.Delay(_timeoutOrderIds.Contains(orderId) ? 5000 : 700, ct);

        // Creating transaction fileName
        string transactionId = "TXN-" + Guid.NewGuid().ToString("N")[..12].ToUpper();

        await AddRecordAsync(orderId, amount, PaymentStatus.Captured, transactionId, ct);


        // =========== we have replaced this code by addrecordAsync function =============
        //write in file
        //await JsonFileStore.UpdateJsonFileAsync<PaymentTransaction, bool>(DataFiles.payments, payments =>
        //{
        //    payments.Add(new PaymentTransaction
        //    {
        //        TransactionId = transactionId,
        //        Amount = amount,
        //        OrderId = orderId,
        //        Status = PaymentStatus.Captured,
        //        CreatedAt = DateTime.Now
        //    });
        //    return true;

        //}, ct);

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

    // the payment record is written after the last delay
    private static Task AddRecordAsync(int orderId, decimal amount, PaymentStatus status, string? transactionId, CancellationToken ct) =>
        JsonFileStore.UpdateJsonFileAsync<PaymentTransaction, bool>(DataFiles.payments, payments =>
        {
            payments.Add(new PaymentTransaction
            {
                OrderId = orderId,
                Amount = amount,
                Status = status,
                TransactionId = transactionId,
                CreatedAt = DateTime.Now
            });
            return true;
        }, ct);
}


