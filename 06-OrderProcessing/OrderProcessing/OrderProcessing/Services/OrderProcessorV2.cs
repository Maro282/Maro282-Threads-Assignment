using OrderProcessing.Models;
using OrderProcessing.Services;

namespace OrderProcessing.Repositories;

public class OrderProcessorV2
{

    private readonly ProductRepository _productRepository;
    private readonly OrderRepository _orderRepository;
    private readonly OrderPricer _orderPricer;
    private readonly InvoiceGenerator _invoiceGenerator;

    public OrderProcessorV2(ProductRepository productRepository, OrderRepository orderRepository, OrderPricer orderPricer, InvoiceGenerator invoiceGenerator)
    {

        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderPricer = orderPricer;
        _invoiceGenerator = invoiceGenerator;

    }

    public async Task<bool> ProcessValidatedOrderAsync(Order order, CancellationToken ct)
    {

        // Reserve Stock of each item with safe reservation
        List<OrderItem> reservedItems = new();

        foreach (var item in order.Items)
        {

            bool reserved = await _productRepository.ReserveStockAsync(item.ProductId, item.Quantity, ct);

            if (!reserved)
            {

                foreach (var reservedItem in reservedItems)
                {
                    await _productRepository.ReleaseStockAsync(reservedItem.ProductId, reservedItem.Quantity, ct);
                }

                Product? product = await _productRepository.GetByIdAsync(item.ProductId, ct);

                order.Status = OrderStatus.Rejected;

                order.FailureReason = $"Insufficient stock for '{product?.Name}'";

                await _orderRepository.SaveAsync(order, ct);

                return false;
            }

            reservedItems.Add(item);

        }

        // calculate order
        await _orderPricer.PriceOrderAsync(order, ct);

        // Create the invoice for order
        await _invoiceGenerator.GenerateAsync(order, ct);

        // Complete order
        order.Status = OrderStatus.Completed;

        // Save order
        await _orderRepository.SaveAsync(order, ct);

        return true;

    }


}