
using System.Diagnostics;
using System.Globalization;
using OrderProcessing.Models;
using OrderProcessing.Repositories;

namespace OrderProcessing.Runners;

public class Phase1Runner
{
    private const string Name = nameof(Phase1Runner);

    private readonly IncomingOrderRepository _incomingOrders;
    private readonly OrderRepository _orders;
    private readonly OrderValidator _validator;
    private readonly SyncOrderValidator _syncValidator;

    public Phase1Runner(
        IncomingOrderRepository incomingOrders,
        OrderRepository orders,
        OrderValidator validator,
        SyncOrderValidator syncValidator)
    {

        _incomingOrders = incomingOrders;
        _orders = orders;
        _validator = validator;
        _syncValidator = syncValidator;
    }

    public async Task RunAsync(CancellationToken ct)
    {

        // 1- Reset & load 20 orders
        await DataSeeder.ResetAsync(ct);
        List<Order> orders = await _incomingOrders.GetAllAsync(ct);

        //2- validate each order
        Log.Info(Name, "=== Sync run: validating the orders one by one ===");
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (var order in orders)
        {
            _syncValidator.ValidateOrder(order);
        }
        stopwatch.Stop();
        double syncSeconds = stopwatch.Elapsed.TotalSeconds;


        // 3) reset again
        await DataSeeder.ResetAsync(ct);

        // 4) async run: foreach -> await ValidateOrderAsync -> await SaveAsync
        Log.Info(Name, "=== Async run: validating the orders one by one ===");
        var validationOnly = new Stopwatch();   // time spent only inside ValidateOrderAsync
        stopwatch.Restart();
        foreach (Order order in orders)
        {
            validationOnly.Start();
            await _validator.ValidateOrderAsync(order, ct);
            validationOnly.Stop();

            await _orders.SaveAsync(order, ct);
        }
        stopwatch.Stop();
        double asyncSeconds = stopwatch.Elapsed.TotalSeconds;


        // 5) summary: the counts are read back from orders.json, not from memory

        List<Order> all = await _orders.GetAllAsync(ct);
        List<Order> validated = await _orders.GetByStatusAsync(OrderStatus.Validated, ct);
        List<Order> rejected = await _orders.GetByStatusAsync(OrderStatus.Rejected, ct);

        Console.WriteLine();
        Console.WriteLine("===== Validation Summary =====");
        Console.WriteLine($"Total Orders: {all.Count}");
        Console.WriteLine($"Validated: {validated.Count}");
        Console.WriteLine($"Rejected: {rejected.Count}");
        foreach (Order order in rejected.OrderBy(o => o.Id))
        {
            Console.WriteLine($" - Order #{order.Id}: {order.FailureReason}");
        }
        Console.WriteLine($"Sync Time: {Seconds(syncSeconds)} seconds");
        Console.WriteLine($"Async Time: {Seconds(asyncSeconds)} seconds");
        Console.WriteLine($"orders.json now contains {all.Count} records.");

    }

    private static string Seconds(double value) => value.ToString("F1", CultureInfo.InvariantCulture);


}
