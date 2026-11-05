
using OrderProcessing.Exceptions;
using OrderProcessing.Models;
using OrderProcessing.Shared;

namespace OrderProcessing.Services;

public class SyncOrderProcessor
{
    private readonly ISyncCheckoutServices _services;

    public SyncOrderProcessor(ISyncCheckoutServices services)
    {
        _services = services;
    }

    public bool ProcessOrder(Order order)
    {
        ThreadUsageTracker.RecordCurrentThread();

        Customer customer;
        bool stockReserved = false;
        string? transactionId = null;

        try
        {
            // Step 1: Customer
            customer = _services.GetCustomer(order.CustomerId);

            if (!customer.IsActive)
                throw new InvalidOperationException(
                    "Customer is inactive");

            // Step 2: Inventory
            bool reserved = _services.ReserveStock(order);

            if (!reserved)
                throw new InvalidOperationException(
                    "Insufficient stock");

            stockReserved = true;

            // Step 3: Pricing
            _services.CalculateTotal(order);

            // Step 4: Payment
            PaymentResult payment = _services.ProcessPayment(
                order.Id, order.TotalAmount);

            if (!payment.Success)
                throw new PaymentDeclinedException(
                    payment.Message ?? "Card declined by issuer");

            transactionId = payment.TransactionId;
            order.TransactionId = transactionId;

            // Step 5: Save
            order.Status = OrderStatus.Completed;
            _services.SaveOrder(order);

            // Step 6: Email
            // Email failure must not change the order status.
            try
            {
                _services.SendConfirmation(order, customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Order #{order.Id}: Email failed: {ex.Message}");
            }

            return true;
        }
        catch (CustomerNotFoundException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;
        }
        catch (InvalidCouponException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;
        }
        catch (PaymentDeclinedException ex)
        {
            order.Status = OrderStatus.PaymentFailed;
            order.FailureReason = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;
        }
        catch (Exception ex) when (transactionId != null)
        {
            _services.Refund(transactionId);

            if (stockReserved)
                _services.ReleaseStock(order);

            order.Status = OrderStatus.Failed;
            order.FailureReason =
                $"Database save failed after payment; " +
                $"payment {transactionId} refunded. {ex.Message}";

            _services.SaveOrder(order);
            return false;
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            order.FailureReason = ex.Message;
        }

        if (stockReserved)
            _services.ReleaseStock(order);

        _services.SaveOrder(order);

        return false;
    }
}
