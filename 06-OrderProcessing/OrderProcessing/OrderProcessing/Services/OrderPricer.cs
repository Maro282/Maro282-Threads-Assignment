
using OrderProcessing.Models;

namespace OrderProcessing.Services;

public class OrderPricer
{

    private readonly PricingService _pricingService;

    public OrderPricer(PricingService pricingService)
    {
        _pricingService = pricingService;
    }


    public async Task<decimal> PriceOrderAsync(Order order, CancellationToken ct)
    {

        decimal subTotal = await _pricingService.CalculateSubTotalAsync(order, ct);
        Task<decimal> discount = _pricingService.GetDiscountAsync(order, ct);
        Task<decimal> shippingCost = _pricingService.CalculateShippingCostAsync(order, ct);

        await Task.WhenAll(discount, shippingCost);

        //return await subTotal - await discount + shippingCost;




    }


}
