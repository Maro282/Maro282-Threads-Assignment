
using System.Diagnostics;
using OrderProcessing.Models;
using OrderProcessing.Repositories;
using OrderProcessing.Services;

namespace OrderProcessing.Runners;

public class Phase3Runner
{
    private readonly OrderProcessor _orderProcessor;
    private readonly OrderRepository _orderRepository;
    private readonly Func<CancellationToken, Task> _resetAsync;

    public Phase3Runner(
        OrderProcessor orderProcessor,
        OrderRepository orderRepository,
        Func<CancellationToken, Task> resetAsync)
    {
        _orderProcessor = orderProcessor;
        _orderRepository = orderRepository;
        _resetAsync = resetAsync;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await _resetAsync(ct);

        List<Order> orders = await _orderRepository.GetAllAsync(ct);

        var stopwatch = Stopwatch.StartNew();

        // Start all 20 orders concurrently.
        List<Task<bool>> tasks = [];

        foreach (Order order in orders)
        {
            tasks.Add(
                _orderProcessor.ProcessOrderAsync(order, ct));
        }

        Console.WriteLine(
            $"Started {tasks.Count} orders.");

        // Await each task individually.
        foreach (Task<bool> task in tasks)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Unexpected processing exception: {ex.Message}");
            }
        }

        stopwatch.Stop();

        List<Order> finalOrders =
            await _orderRepository.GetAllAsync(CancellationToken.None);

        Console.WriteLine();
        Console.WriteLine("========== FINAL ORDERS ==========");

        Console.WriteLine(
            $"{"Order",-10} {"Status",-18} {"Total",-12} Reason");

        foreach (Order order in finalOrders.OrderBy(o => o.Id))
        {
            Console.WriteLine(
                $"#{order.Id,-9} " +
                $"{order.Status,-18} " +
                $"{order.TotalAmount,-12:F2} " +
                $"{order.FailureReason}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total time: {stopwatch.Elapsed}");

        Console.WriteLine(
            $"Total records in orders.json: {finalOrders.Count}");

        Console.WriteLine(
            $"Completed: {finalOrders.Count(o => o.Status == OrderStatus.Completed)}");

        Console.WriteLine(
            $"Rejected: {finalOrders.Count(o => o.Status == OrderStatus.Rejected)}");

        Console.WriteLine(
            $"PaymentFailed: {finalOrders.Count(o => o.Status == OrderStatus.PaymentFailed)}");

        Console.WriteLine(
            $"Failed: {finalOrders.Count(o => o.Status == OrderStatus.Failed)}");
    }
}
