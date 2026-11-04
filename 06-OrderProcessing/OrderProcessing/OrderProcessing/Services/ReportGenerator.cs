using OrderProcessing.Models;
using OrderProcessing.Repositories;
using System.Text.Json;

namespace OrderProcessing.Services;


public class ReportGenerator
{

    private readonly ProductRepository _productRepository;
    private readonly OrderRepository _orderRepository;


    public ReportGenerator(ProductRepository productRepository, OrderRepository orderRepository)
    {

        _productRepository = productRepository;
        _orderRepository = orderRepository;

    }


    public async Task<Report> GenerateAsync(CancellationToken ct)
    {

        Log.Info(nameof(ReportGenerator), "Generating report...");

        // load data
        List<Order> allOrders = await _orderRepository.GetAllAsync(ct);

        // Count orders types
        int totalOrders = allOrders.Count;
        int orderCompletedCount = allOrders.Count(o => o.Status == OrderStatus.Completed);
        int orderRejectedCount = allOrders.Count(o => o.Status == OrderStatus.Rejected);
        int orderFailedCount = allOrders.Count(o => o.Status == OrderStatus.Failed);
        int paymentFieldOrdersCount = allOrders.Count(o => o.Status == OrderStatus.PaymentFailed);

        // calculating total revenue

        List<Order> completedOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();
        decimal totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        decimal averageOrderValue = orderCompletedCount == 0 ? 0 : totalRevenue / orderCompletedCount;

        // Getting the top selling
        var topSelling = completedOrders.SelectMany(o => o.Items).GroupBy(item => item.ProductId).Select(group => new
        {
            productId = group.Key,
            quantity = group.Sum(item => item.Quantity)
        }).OrderByDescending(x => x.quantity).ThenBy(x => x.productId).FirstOrDefault();

        string topSellingProduct = string.Empty;
        int topSellingQuantity = 0;

        if (topSelling is not null)
        {
            var product = await _productRepository.GetByIdAsync(topSelling.productId, ct);

            topSellingProduct = product?.Name ?? "Unknown Product";
            topSellingQuantity = topSelling.quantity;

        }

        // creating report obj
        var report = new Report
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            CompletedCount = orderCompletedCount,
            RejectedCount = orderRejectedCount,
            FailedCount = orderFailedCount,
            PaymentFailedCount = paymentFieldOrdersCount,
            TopSellingProduct = topSellingProduct,
            TopSellingQuantity = topSellingQuantity,
            AverageOrderValue = averageOrderValue,
            GeneratedAt = DateTime.Now
        };

        // create reports directory

        string reportsDirectory = Path.Combine(DataFiles.DataFolder, "reports");
        Directory.CreateDirectory(reportsDirectory);

        // creating file path
        string generalReportPath = Path.Combine(reportsDirectory, $"report_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

        // Saving report file
        await File.WriteAllTextAsync(generalReportPath, json, ct);

        Log.Info(nameof(ReportGenerator),
           $"Report saved: report_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        return report;


    }

}