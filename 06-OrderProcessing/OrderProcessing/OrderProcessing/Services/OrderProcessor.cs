
using OrderProcessing.Exceptions;
using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using OrderProcessing.Repositories;
using OrderProcessing.Shared;
using System.Diagnostics;

namespace OrderProcessing.Services;

public class OrderProcessor
{
    private readonly CustomerService _customerService;
    private readonly InventoryService _inventoryService;
    private readonly PricingService _pricingService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly OrderRepository _orderRepository;
    private readonly EmailRetryService _emailService;

    public OrderProcessor(
        CustomerService customerService,
        InventoryService inventoryService,
        PricingService pricingService,
        IPaymentGateway paymentGateway,
        OrderRepository orderRepository,
        EmailRetryService emailService
         )
    {
        _customerService = customerService;
        _inventoryService = inventoryService;
        _pricingService = pricingService;
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _emailService = emailService;
    }

    public async Task<bool> ProcessOrderAsync(
        Order order,
        CancellationToken ct = default)
    {
        Customer? customer = null;
        bool stockReserved = false;
        string? transactionId = null;

        try
        {
            // Step 1: Get customer
            await LogStepAsync(order.Id, "Step 1/6 Customer", async () =>
            {
                customer = await _customerService
                    .GetCustomerAsync(order.CustomerId, ct);

                if (!customer.IsActive)
                    throw new InvalidOperationException(
                        "Customer is inactive");
            });

            // Step 2: Reserve inventory
            await LogStepAsync(order.Id, "Step 2/6 Inventory", async () =>
            {
                bool reserved = await _inventoryService
                    .ReserveStockAsync(order, ct);

                if (!reserved)
                    throw new InvalidOperationException(
                        "Insufficient stock");

                stockReserved = true;
            });

            // Step 3: Calculate total
            await LogStepAsync(order.Id, "Step 3/6 Pricing", async () =>
            {
                await _pricingService.CalculateTotalAsync(order, ct);
            });

            // Step 4: Payment with a 3-second timeout
            await LogStepAsync(order.Id, "Step 4/6 Payment", async () =>
            {
                using var timeoutCts =
                    CancellationTokenSource.CreateLinkedTokenSource(ct);

                timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

                PaymentResult result;

                try
                {
                    result = await _paymentGateway.ProcessPaymentAsync(
                        order.Id,
                        order.TotalAmount,
                        timeoutCts.Token);
                }
                catch (OperationCanceledException)
                    when (!ct.IsCancellationRequested)
                {
                    throw new TimeoutException(
                        "Payment gateway timeout (3s).");
                }

                if (!result.Success)
                {
                    throw new PaymentDeclinedException(
                        result.Message ?? "Card declined by issuer");
                }

                transactionId = result.TransactionId;
                order.TransactionId = transactionId;
            });

            // Step 5: Save completed order
            await LogStepAsync(order.Id, "Step 5/6 Save", async () =>
            {
                order.Status = OrderStatus.Completed;
                order.FailureReason = null;

                await _orderRepository.SaveAsync(order, ct);
            });

            // Step 6: Email must not change the order status.
            await LogStepAsync(order.Id, "Step 6/6 Email", async () =>
            {
                await _emailService.SendConfirmationWithRetryAsync(
                    order, customer!, ct);
            });

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Cancellation is NOT a payment failure.
            if (transactionId is not null)
            {
                await _paymentGateway.RefundAsync(transactionId, CancellationToken.None);
                order.FailureReason =
                    $"Cancelled; payment {transactionId} refunded";
            }

            if (stockReserved)
                await _inventoryService.ReleaseStockAsync(
                    order, CancellationToken.None);

            order.Status = OrderStatus.Pending;

            await _orderRepository.SaveAsync(
                order, CancellationToken.None);

            return false;
        }
        catch (CustomerNotFoundException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            // Inactive customer or insufficient stock.
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
        catch (InvalidCouponException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
        catch (PaymentDeclinedException ex)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
        catch (TimeoutException ex)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
        catch (Exception ex) when (transactionId is not null)
        {
            // A failure after successful payment:
            // refund payment, release inventory, mark Failed.
            await _paymentGateway.RefundAsync(
                transactionId, CancellationToken.None);

            if (stockReserved)
            {
                await _inventoryService.ReleaseStockAsync(
                    order, CancellationToken.None);
            }

            order.Status = OrderStatus.Failed;
            order.FailureReason =
                $"Database save failed after payment; payment {transactionId} refunded. {ex.Message}";

            await _orderRepository.SaveAsync(
                order, CancellationToken.None);

            return false;
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            order.FailureReason = ex.Message;

            await SaveFinalStatusAsync(order, stockReserved);
            return false;
        }
    }

    private async Task SaveFinalStatusAsync(
        Order order,
        bool stockReserved)
    {
        if (stockReserved)
        {
            await _inventoryService.ReleaseStockAsync(
                order, CancellationToken.None);
        }

        await _orderRepository.SaveAsync(
            order, CancellationToken.None);
    }

    private static async Task LogStepAsync(
        int orderId,
        string step,
        Func<Task> action)
    {
        ThreadUsageTracker.RecordCurrentThread();
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[Thread {Environment.CurrentManagedThreadId}] " +
            $"Order #{orderId} -> {step} ...");

        await action();

        stopwatch.Stop();

        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[Thread {Environment.CurrentManagedThreadId}] " +
            $"Order #{orderId} -> {step} OK " +
            $"in {stopwatch.ElapsedMilliseconds} ms");
    }
}
