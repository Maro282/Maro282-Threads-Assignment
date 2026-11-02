

using OrderProcessing.Models;
using System.Xml.Linq;

namespace OrderProcessing.Repositories;

public class ProductRepository
{

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        // Simulating connection with dataBase
        Log.Info(DataFiles.products, "Open Connection . . ");
        await Task.Delay(200, ct);

        // Simulation processing a Query
        Log.Info(nameof(ProductRepository), $"Executing query for Product #{id}...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        //PROCESS
        List<Product> products = await JsonFileStore.ReadFromJsonFileASync<Product>(DataFiles.products, ct);
        Product? product = products.FirstOrDefault(i => i.Id == id);
        Log.Info(nameof(ProductRepository),
                 product is null ? $"Prduct #{id} not found"
                 : $"Product #{id} found ({product.Name}) stock {product.StockQuantity}");

        //RESULT
        return product;

    }

    public async Task<bool> ReserveStockAsync(int productId, int qty, CancellationToken ct)
    {

        //Openning connection
        Log.Info(nameof(ProductRepository), "Oppening Connection .. ");
        await Task.Delay(200);

        //Begin Transation
        Log.Info(nameof(ProductRepository), "Begining Transaction .. ");
        await Task.Delay(100);

        //Updating product
        Log.Info(nameof(ProductRepository), $"Updating stock for Product #{productId} (reserve {qty})...");
        await Task.Delay(400, ct);

        bool reserved = await JsonFileStore
                      .UpdateJsonFileAsync<Product, bool>(DataFiles.products, products =>
                      {
                          Product? product = products.FirstOrDefault(p => p.Id == productId);
                          if (product is null || product.StockQuantity <= 0) return false;
                          product.StockQuantity -= qty;
                          return true;
                      }, ct);

        // Commiting
        Log.Info(nameof(ProductRepository), "Committing transaction...");
        await Task.Delay(200);

        Log.Info(nameof(ProductRepository), reserved ? $"Reserved {qty} of Product #{productId}"
                                           : $"Could not reserve {qty} of Product #{productId}");

        return reserved;
    }



}
