
using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class CouponRepository
{

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct)
    {
        // Simulating connection with dataBase
        Log.Info(DataFiles.coupons, "Open Connection . . ");
        await Task.Delay(200, ct);

        // Simulation processing a Query
        Log.Info(nameof(CouponRepository), $"Executing query for Coupon of code #{code}...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        // Process
        List<Coupon> coupons = await JsonFileStore.ReadFromJsonFileASync<Coupon>(DataFiles.coupons, ct);
        Coupon? result = coupons.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        Log.Info(nameof(CustomerRepository),
                 result is null ? $"Coupon #{code} not found"
                 : $"Coupon #{code} found ({result.DiscountPercent}%)");

        //RESULT
        return result;

    }


}
