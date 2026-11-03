
using OrderProcessing.Models;

namespace OrderProcessing.Services;

public class OrderPricer
{

    private readonly PricingService _pricingService;

    public OrderPricer(PricingService pricingService)
    {
        _pricingService = pricingService;
    }


    public async Task PriceOrderAsync(Order order, CancellationToken ct)
    {
        // Calculate subtotal first
        await _pricingService.CalculateSubTotalAsync(order, ct);

        // Concurrent
        Task<decimal> discountTask = _pricingService.GetDiscountAsync(order, ct);
        Task<decimal> shippingCostTask = _pricingService.CalculateShippingCostAsync(order, ct);

        order.Discount = await discountTask;
        order.ShippingCost = await shippingCostTask;

        order.TotalAmount = Math.Round(
          order.SubTotal - order.Discount + order.ShippingCost,
          2,
          MidpointRounding.AwayFromZero);

    }

    public async Task PriceOrderSequentialAsync(Order order, CancellationToken ct)
    {
        await _pricingService.CalculateSubTotalAsync(order, ct);

        // Sequential
        order.Discount = await _pricingService.GetDiscountAsync(order, ct);
        order.ShippingCost = await _pricingService.CalculateShippingCostAsync(order, ct);


        order.TotalAmount = Math.Round(
          order.SubTotal - order.Discount + order.ShippingCost,
          2,
          MidpointRounding.AwayFromZero);
    }


}
