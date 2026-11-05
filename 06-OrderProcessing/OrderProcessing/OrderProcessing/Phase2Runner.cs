

using OrderProcessing.Models;
using OrderProcessing.Repositories;
using OrderProcessing.Services;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace OrderProcessing;

public class Phase2Runner
{

    private readonly RaceConditionDemo _raceConditionDemo;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly IncomingOrderRepository _incomingOrderRepository;

    private readonly OrderPricer _orderPricer;
    private readonly OrderValidator _orderValidator;
    private readonly OrderProcessorV2 _orderProcessorV2;
    private readonly ReportGenerator _reportGenerator;

    public Phase2Runner(RaceConditionDemo raceConditionDemo,
                        OrderRepository orderRepository,
                        IncomingOrderRepository incomingOrderRepository,
                        OrderPricer orderPricer,
                        OrderValidator orderValidator,
                        OrderProcessorV2 orderProcessorV2,
                        ProductRepository productRepository,
                        ReportGenerator reportGenerator)
    {
        _raceConditionDemo = raceConditionDemo;
        _orderRepository = orderRepository;
        _incomingOrderRepository = incomingOrderRepository;
        _orderPricer = orderPricer;
        _orderValidator = orderValidator;
        _orderProcessorV2 = orderProcessorV2;
        _productRepository = productRepository;
        _reportGenerator = reportGenerator;
    }



    public async Task RunAsync(CancellationToken ct)
    {
        // 1- Reset data
        Log.Info(nameof(Phase2Runner), "Reset Data . . ");
        await DataSeeder.ResetAsync(ct);

        // 2-  Run race condition demo
        Log.Info(nameof(Phase2Runner), "Running Race Condition Demo . . . ");
        await _raceConditionDemo.RunAsync(ct);

        // 3 - Resetting data after race condition
        Log.Info(nameof(Phase2Runner), "Resetting data after race condition demo ");
        await DataSeeder.ResetAsync(ct);

        // 4- Sequential vs Parallel Pricing on order 1
        Log.Info(nameof(Phase2Runner), "Sequential & Concurrent pricing for order #1  ");
        await PricingComparison(ct);

        // 5 - validating orders concurrently
        await ValidatingOrdersConcurrently(ct);

        // 6 - process validated orders concurrently
        await ProcessValidatedordersConcurrently(ct);

        // 7- general report

        Console.WriteLine("Generating report...");

        Report report = await _reportGenerator.GenerateAsync(ct);


        Console.WriteLine($"Total Orders: {report.TotalOrders}");
        Console.WriteLine($"Total Revenue: {report.TotalRevenue}");
        Console.WriteLine($"Completed: {report.CompletedCount}");
        Console.WriteLine($"Rejected: {report.RejectedCount}");
        Console.WriteLine($"Failed: {report.FailedCount}");
        Console.WriteLine($"Payment Failed: {report.PaymentFailedCount}");

        Console.WriteLine(
            $"Top Selling Product: {report.TopSellingProduct}");

        Console.WriteLine(
            $"Top Selling Quantity: {report.TopSellingQuantity}");

        Console.WriteLine(
            $"Average Order Value: {report.AverageOrderValue}");

    }

    public async Task PricingComparison(CancellationToken ct)
    {

        List<Order> orders = await _incomingOrderRepository.GetAllAsync(ct);

        Order? originalOrder = orders.FirstOrDefault(o => o.Id == 1);

        if (originalOrder is null)
        {
            Log.Error(nameof(Phase2Runner), "Order #1 was not found.");
            return;
        }

        var orderCopy1 = CloneOrder(originalOrder);
        var orderCopy2 = CloneOrder(originalOrder);

        // Sequential pricing
        Stopwatch stopwatch = Stopwatch.StartNew();

        await _orderPricer.PriceOrderSequentialAsync(
            orderCopy1, ct);

        stopwatch.Stop();

        double sequentialSeconds =
            stopwatch.Elapsed.TotalSeconds;

        // Parallel pricing
        stopwatch.Restart();

        await _orderPricer.PriceOrderAsync(
            orderCopy2, ct);

        stopwatch.Stop();

        double parallelSeconds =
            stopwatch.Elapsed.TotalSeconds;

        Console.WriteLine(
            $"Sequential Time: {Seconds(sequentialSeconds)} seconds");

        Console.WriteLine(
            $"Parallel Time: {Seconds(parallelSeconds)} seconds");



    }


    public async Task ValidatingOrdersConcurrently(CancellationToken ct)
    {
        List<Order> orders = await _incomingOrderRepository.GetAllAsync(ct);
        Task<bool>[] tasks = orders.Select(async (o) =>
        {
            bool validated = await _orderValidator.ValidateOrderAsync(o, ct);
            await _orderRepository.SaveAsync(o, ct);
            return validated;
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    public async Task ProcessValidatedordersConcurrently(CancellationToken ct)
    {
        List<Order>? ValidatedOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Validated, ct);

        Console.WriteLine($" Validated orders : {ValidatedOrders.Count}");

        List<Task<bool>> tasks = new();
        foreach (Order order in ValidatedOrders)
        {
            tasks.Add(_orderProcessorV2.ProcessValidatedOrderAsync(order, ct));
        }


        // Print initial task statuses immediately.
        Console.WriteLine("\n--- Initial Task Status ---");

        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine(
                $"Order #{ValidatedOrders[i].Id}: {tasks[i].Status}");
        }

        // Wait one second.
        await Task.Delay(1000, ct);

        // Print task statuses after one second.
        Console.WriteLine("\n--- Task Status After 1 Second ---");

        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine(
                $"Order #{ValidatedOrders[i].Id}: {tasks[i].Status}");
        }

        //Await each task in a foreach.
        //A failing order must not stop the others: print its task Status, mark it
        //Failed with the error message, release its stock, save it, and continue.

        for (int i = 0; i < tasks.Count; i++)
        {
            Task<bool> task = tasks[i];
            Order order = ValidatedOrders[i];
            try
            {
                bool succeeded = await task;
                Console.WriteLine($"Order : {order.Id} {task.Status} | Succeeded : {succeeded}");
            }
            catch (Exception ex)
            {
                // log error
                Log.Error(nameof(Phase2Runner), $"Order #{order.Id} failed: {ex.Message}");

                // mark status as faild
                order.Status = OrderStatus.Failed;
                order.FailureReason = ex.Message;

                //release stock
                foreach (OrderItem item in order.Items)
                {
                    await _productRepository.ReleaseStockAsync(item.ProductId, item.Quantity, ct);

                }

                // Save order 
                await _orderRepository.SaveAsync(order, ct);

                Console.WriteLine(
                   $"Order #{order.Id}: Failed | {ex.Message}");
            }
        }


        // Final task statuses
        Console.WriteLine("\n--- Final Task Status ---");

        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine(
                $"Order #{ValidatedOrders[i].Id}: {tasks[i].Status}");
        }
    }

    // Helper methods
    public Order CloneOrder(Order order)
    {
        return new Order
        {
            Id = order.Id,
            CustomerId = order.CustomerId,

            Items = order.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList(),

            CouponCode = order.CouponCode,
            SubTotal = order.SubTotal,
            Discount = order.Discount,
            ShippingCost = order.ShippingCost,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            FailureReason = order.FailureReason,
            TransactionId = order.TransactionId
        };
    }
    private static string Seconds(double value) => value.ToString("F1", CultureInfo.InvariantCulture);

}
