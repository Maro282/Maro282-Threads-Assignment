

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class SyncCouponRepository
{

    public Coupon GetByCode(string code)
    {
        // Simulating connection with dataBase
        Log.Info(DataFiles.coupons, "Open Connection . . ");
        Thread.Sleep(200);

        // Simulation processing a Query
        Log.Info(nameof(CouponRepository), $"Executing query for Coupon of code #{code}...");
        Thread.Sleep(Random.Shared.Next(300, 601));

        // Process
        List<Coupon> coupons = JsonFileStore.ReadFromJsonFile<Coupon>(DataFiles.coupons);
        Coupon? result = coupons.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        Log.Info(nameof(CustomerRepository),
                 result is null ? $"Coupon #{code} not found"
                 : $"Coupon #{code} found ({result.DiscountPercent}%)");

        //RESULT
        return result;

    }

}
