

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class SyncProductRepository
{

    public Product? GetById(int id)
    {
        // Simulating connection with dataBase
        Log.Info(DataFiles.products, "Open Connection . . ");
        Thread.Sleep(200);

        // Simulation processing a Query
        Log.Info(nameof(ProductRepository), $"Executing query for Product #{id}...");
        Thread.Sleep(Random.Shared.Next(300, 601));

        //PROCESS
        List<Product> products = JsonFileStore.ReadFromJsonFile<Product>(DataFiles.products);
        Product? product = products.FirstOrDefault(i => i.Id == id);
        Log.Info(nameof(ProductRepository),
                 product is null ? $"Prduct #{id} not found"
                 : $"Product #{id} found ({product.Name}) stock {product.StockQuantity}");

        //RESULT
        return product;

    }

    public bool ReserveStock(int productId, int qty)
    {

        //Openning connection
        Log.Info(nameof(ProductRepository), "Oppening Connection .. ");
        Thread.Sleep(200);

        //Begin Transation
        Log.Info(nameof(ProductRepository), "Begining Transaction .. ");
        Thread.Sleep(100);

        //Updating product
        Log.Info(nameof(ProductRepository), $"Updating stock for Product #{productId} (reserve {qty})...");
        Thread.Sleep(400);

        bool reserved = JsonFileStore
                      .UpdateJsonFile<Product, bool>(DataFiles.products, products =>
                      {
                          Product? product = products.FirstOrDefault(p => p.Id == productId);
                          if (product is null || product.StockQuantity <= 0) return false;
                          product.StockQuantity -= qty;
                          return true;
                      });

        // Commiting
        Log.Info(nameof(ProductRepository), "Committing transaction...");
        Thread.Sleep(200);

        Log.Info(nameof(ProductRepository), reserved ? $"Reserved {qty} of Product #{productId}"
                                           : $"Could not reserve {qty} of Product #{productId}");

        return reserved;
    }



}
