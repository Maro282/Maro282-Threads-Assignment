
using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class RaceConditionDemo
{

    private readonly ProductRepository _productRepository;

    public RaceConditionDemo(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task RunAsync(CancellationToken ct)
    {

        int productId = 4;
        int qty = 1;
        // Reset stock to 1 before the test

        bool updated = await JsonFileStore.UpdateJsonFileAsync<Product, bool>(DataFiles.products, products =>
       {
           Product? currentProduct = products.FirstOrDefault(p => p.Id == productId);
           if (currentProduct is null) return false;
           currentProduct.StockQuantity = qty;
           return true;
       }, ct);

        Log.Info(nameof(RaceConditionDemo),
         "Starting unsafe reservation test...");

        // Run two tasks concurrently
        Task<bool> order10Reservation = _productRepository.ReserveStockUnsafeAsync(productId, qty, ct);
        Task<bool> order11Reservation = _productRepository.ReserveStockUnsafeAsync(productId, qty, ct);

        bool[] result = await Task.WhenAll(order10Reservation, order11Reservation);

        // read Final stock after two tasks
        Product? product = await _productRepository.GetByIdAsync(productId, ct);

        Console.WriteLine("--- Unsafe Reservation Results ---");
        Console.WriteLine($"Order #10: {result[0]}");
        Console.WriteLine($"Order #11: {result[1]}");
        Console.WriteLine($"Final Stock: {product?.StockQuantity}");



    }


}
