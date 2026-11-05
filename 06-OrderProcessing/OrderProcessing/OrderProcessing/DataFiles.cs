

namespace OrderProcessing;

public static class DataFiles
{
    // Thre folder that all files live in
    public static readonly string DataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
    public const string customers = "customers.json";
    public const string products = "products.json";
    public const string coupons = "coupons.json";
    public const string incoming_orders = "incoming_orders.json";
    public const string orders = "orders.json";
    public const string payments = "payments.json";
    public const string emails = "emails.json";
    public const string emailRetry = "email_retry.json";

}
