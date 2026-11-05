
using OrderProcessing.Models;

namespace OrderProcessing.Services;

public interface ISyncCheckoutServices
{
    Customer GetCustomer(int customerId);

    bool ReserveStock(Order order);

    void ReleaseStock(Order order);

    void CalculateTotal(Order order);

    PaymentResult ProcessPayment(int orderId, decimal amount);

    void Refund(string transactionId);

    void SaveOrder(Order order);

    void SendConfirmation(Order order, Customer customer);
}
