

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class IncomingOrderRepository
{

    private readonly string Name = nameof(IncomingOrderRepository);

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {

        Log.Info(Name, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(Name, "Executing query for incoming orders...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        List<Order> orders = await JsonFileStore.ReadFromJsonFileASync<Order>(DataFiles.incoming_orders, ct);

        Log.Info(Name, $"{orders.Count} incoming orders found");
        return orders;
    }

}
