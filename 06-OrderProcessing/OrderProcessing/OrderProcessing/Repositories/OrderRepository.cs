
using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class OrderRepository
{
    private readonly string Name = nameof(OrderRepository);

    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        //Openning connection
        Log.Info(nameof(ProductRepository), "Oppening Connection .. ");
        await Task.Delay(200);

        //Begin Transation
        Log.Info(Name, "Begining Transaction .. ");
        await Task.Delay(100);

        //Updating/Adding Order
        Log.Info(Name, $"Saving Order #{order.Id} ({order.Status})...");
        await Task.Delay(400, ct);

        bool saved = await JsonFileStore
                      .UpdateJsonFileAsync<Order, bool>(DataFiles.orders, orders =>
                      {
                          int index = orders.FindIndex(o => o.Id == order.Id);
                          if (index >= 0) orders[index] = order;
                          else orders.Add(order);
                          return true;
                      }, ct);

        // Commiting
        Log.Info(Name, "Committing transaction...");
        await Task.Delay(200);

        Log.Info(Name, $"Order #{order.Id} saved");
    }


    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {

        Log.Info(Name, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(Name, "Getting all orders...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        List<Order> orders = await JsonFileStore.ReadFromJsonFileASync<Order>(DataFiles.orders, ct);

        Log.Info(Name, $"{orders.Count} found");
        return orders;
    }


    public async Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct)
    {
        Log.Info(Name, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(Name, $"Getting all orders of status {status}...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        List<Order> orders = await JsonFileStore.ReadFromJsonFileASync<Order>(DataFiles.orders, ct);

        List<Order> FilteredOrders = orders.FindAll(o => o.Status == status);

        Log.Info(Name, $"{FilteredOrders.Count} orders found with status {status}");
        return FilteredOrders;



    }



}
