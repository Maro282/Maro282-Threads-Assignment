using OrderProcessing.Models;
using OrderProcessing.Repositories;

namespace OrderProcessing.Services;

public class InventoryService
{

    private readonly ProductRepository _productRepository;


    public InventoryService(ProductRepository productRepository)
    {
        _productRepository = productRepository;

    }



    public async Task<bool> ReserveStockAsync(Order order, CancellationToken ct)
    {

        List<OrderItem> reservedItems = new();


        try
        {

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
                    order.FailureReason = $"Insufficient stock for '{product?.Name ?? "#" + item.ProductId}'";

                    return false;

                }
                reservedItems.Add(item);

            }
        }
        catch
        {
            await ReleaseStockAsync(order, ct);
            throw;
        }


        return true;
    }





    public async Task ReleaseStockAsync(Order order, CancellationToken ct)
    {

        foreach (var item in order.Items)
        {
            await _productRepository.ReleaseStockAsync(item.ProductId, item.Quantity, ct);
        }

    }


}

