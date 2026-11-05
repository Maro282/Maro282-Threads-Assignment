
using System.Diagnostics;
using OrderProcessing.Models;
using OrderProcessing.Services;
using OrderProcessing.Shared;

namespace OrderProcessing.Runners;

public class SyncAsyncComparisonRunner
{
    private readonly Func<CancellationToken, Task> _resetAsync;
    private readonly Action _resetSync;

    private readonly Func<CancellationToken, Task<List<Order>>> _getOrdersAsync;
    private readonly Func<List<Order>> _getOrdersSync;

    private readonly OrderProcessor _asyncProcessor;
    private readonly SyncOrderProcessor _syncProcessor;

    public SyncAsyncComparisonRunner(
        Func<CancellationToken, Task> resetAsync,
        Action resetSync,
        Func<CancellationToken, Task<List<Order>>> getOrdersAsync,
        Func<List<Order>> getOrdersSync,
        OrderProcessor asyncProcessor,
        SyncOrderProcessor syncProcessor)
    {
        _resetAsync = resetAsync;
        _resetSync = resetSync;
        _getOrdersAsync = getOrdersAsync;
        _getOrdersSync = getOrdersSync;
        _asyncProcessor = asyncProcessor;
        _syncProcessor = syncProcessor;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("========== SYNC VERSION ==========");

        _resetSync();

        List<Order> syncOrders = _getOrdersSync()
            .Where(o => o.Id >= 1 && o.Id <= 10)
            .OrderBy(o => o.Id)
            .ToList();

        ThreadUsageTracker.Reset();

        var syncStopwatch = Stopwatch.StartNew();

        List<Thread> threads = [];

        foreach (Order order in syncOrders)
        {
            Thread thread = new(() =>
            {
                _syncProcessor.ProcessOrder(order);
            });

            threads.Add(thread);
            thread.Start();
        }

        // Join every thread.
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        syncStopwatch.Stop();

        int syncThreadCount = ThreadUsageTracker.Count;

        Console.WriteLine(
            $"Sync time: {syncStopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"Sync threads used: {syncThreadCount}");

        Console.WriteLine();

        Console.WriteLine("========== ASYNC VERSION ==========");

        await _resetAsync(ct);

        List<Order> asyncOrders = await _getOrdersAsync(ct);

        asyncOrders = asyncOrders
            .Where(o => o.Id >= 1 && o.Id <= 10)
            .OrderBy(o => o.Id)
            .ToList();

        ThreadUsageTracker.Reset();

        int threadPoolBefore = ThreadPool.ThreadCount;

        var asyncStopwatch = Stopwatch.StartNew();

        List<Task<bool>> tasks = [];

        foreach (Order order in asyncOrders)
        {
            tasks.Add(
                _asyncProcessor.ProcessOrderAsync(order, ct));
        }

        foreach (Task<bool> task in tasks)
        {
            await task;
        }

        asyncStopwatch.Stop();

        int asyncThreadCount = ThreadUsageTracker.Count;
        int threadPoolAfter = ThreadPool.ThreadCount;

        Console.WriteLine(
            $"Async time: {asyncStopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"Async threads used: {asyncThreadCount}");

        Console.WriteLine(
            $"ThreadPool.ThreadCount before: {threadPoolBefore}");

        Console.WriteLine(
            $"ThreadPool.ThreadCount after: {threadPoolAfter}");

        Console.WriteLine();
        Console.WriteLine("========== COMPARISON ==========");

        Console.WriteLine(
            $"Sync:  {syncStopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"Async: {asyncStopwatch.ElapsedMilliseconds} ms");
    }
}
