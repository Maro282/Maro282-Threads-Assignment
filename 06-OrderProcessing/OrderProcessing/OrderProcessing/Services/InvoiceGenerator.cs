using OrderProcessing.Models;

namespace OrderProcessing.Services;

public class InvoiceGenerator
{


    public async Task GenerateAsync(Order order, CancellationToken ct)
    {

        Log.Info(nameof(InvoiceGenerator),
                  $"Generating invoice for Order #{order.Id}...");



        Task invoice = Task.Run(() =>
        {
            long result = 0;
            for (int i = 0; i < 20_000_000; i++)
            {
                result = result % 10;
            }

        });


        // 3. Create invoice content
        string invoiceContent = $"""
            Invoice
            =========================
            Order ID: {order.Id}
            Customer ID: {order.CustomerId}
            SubTotal: {order.SubTotal}
            Discount: {order.Discount}
            Shipping Cost: {order.ShippingCost}
            Total Amount: {order.TotalAmount}
            Created At: {order.CreatedAt}
            =========================
            """;



        // File save

        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));
        string invoicesDirectory = Path.Combine(DataFiles.DataFolder, "invoices");
        Directory.CreateDirectory(invoicesDirectory);
        string invoiceFilePath = Path.Combine(invoicesDirectory, $"invoice_{order.Id}.txt");

        await File.WriteAllTextAsync(
          invoiceFilePath,
          invoiceContent,
          ct);

        Log.Info(nameof(InvoiceGenerator),
            $"Invoice generated for Order #{order.Id}");

    }





}