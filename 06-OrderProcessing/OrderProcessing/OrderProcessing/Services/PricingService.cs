

using OrderProcessing.Exceptions;
using OrderProcessing.Models;
using OrderProcessing.Repositories;

namespace OrderProcessing.Services;

public class PricingService
{

    private const string Name = nameof(PricingService);

    private readonly CustomerRepository _customerRepository;
    private readonly CouponRepository _couponRepository;
    private readonly HashSet<int> _failShippingForOrderIds;


    public PricingService(CustomerRepository customerRepository, CouponRepository couponRepository, IEnumerable<int> failShippingForOrderIds)
    {
        _customerRepository = customerRepository;
        _couponRepository = couponRepository;
        _failShippingForOrderIds = new HashSet<int>(failShippingForOrderIds);
    }

    public async Task<decimal> CalculateSubTotalAsync(Order order, CancellationToken ct)
    {

        //collect all products from products.json

        Log.Info(Name, $"Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(Name, $" Executing query for product prices...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        List<Product> allProducts = await JsonFileStore.ReadFromJsonFileASync<Product>(DataFiles.products, ct);


        // update unit price of these order items
        decimal subtotal = 0m;
        foreach (var item in order.Items)
        {

            Product? product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
            if (product is null) throw new InvalidOperationException($"Product {product?.Id} not found");

            item.UnitPrice = product.Price;
            subtotal += item.UnitPrice * item.Quantity;

        }

        order.SubTotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        return order.SubTotal;
    }





    public async Task<decimal> GetDiscountAsync(Order order, CancellationToken ct)
    {

        Log.Info(Name, $"Order #{order.Id} -> Discount API: DNS/handshake...");
        await Task.Delay(150, ct);

        Log.Info(Name, $"Order #{order.Id} -> Discount API: sending request...");
        await Task.Delay(300, ct);

        // Get order customer 
        Customer? customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);
        if (customer is null)
            throw new InvalidOperationException($"Customer #{order.CustomerId} not found");

        // get order coupon
        Coupon? coupon = order.CouponCode is null
            ? null
            : await _couponRepository.GetByCodeAsync(order.CouponCode, ct);



        // calculate Discount %

        Log.Info(Name, $"Order #{order.Id} -> Discount API: waiting for response...");
        await Task.Delay(Random.Shared.Next(500, 1501), ct);

        // Switch expression to handle loyality
        int loyaltyPercent = customer.LoyaltyTier switch
        {
            LoyaltyTier.Gold => 10,
            LoyaltyTier.Silver => 5,
            _ => 0
        };

        int couponPercent = coupon?.DiscountPercent ?? 0;

        decimal discountPercentage = Math.Min(loyaltyPercent + couponPercent, 30); // capped as demonded

        // Calculate Discount
        decimal discount = await CalculateSubTotalAsync(order, ct) * discountPercentage / 100m;

        return Math.Round(discount, 2, MidpointRounding.AwayFromZero);
    }


    public async Task<decimal> CalculateShippingCostAsync(Order order, CancellationToken ct)
    {

        Log.Info(Name, $"Order #{order.Id} -> Shipping API: DNS/handshake...");
        await Task.Delay(150, ct);

        // injected failure: throws right after the handshake step
        if (_failShippingForOrderIds.Contains(order.Id))
        {
            Log.Error(Name, $"Order #{order.Id} -> Shipping API: provider unavailable");
            throw new ShippingProviderUnavailableException("Shipping provider unavailable");
        }


        Log.Info(Name, $"Order #{order.Id} -> Shipping API: sending request...");
        await Task.Delay(300, ct);

        Log.Info(Name, $"Order #{order.Id} -> Shipping API: waiting for response...");
        await Task.Delay(Random.Shared.Next(500, 1501), ct);

        //collect all products from products.json
        List<Product> allProducts = await JsonFileStore.ReadFromJsonFileASync<Product>(DataFiles.products, ct);

        // calculate total weight
        decimal totalWeight = 0m;
        int totalQuantity = 0;
        foreach (var item in order.Items)
        {
            Product? product = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
            if (product is null)
                throw new InvalidOperationException($"Product #{item.ProductId} not found");

            totalWeight += product.WeightKg * item.Quantity;

            totalQuantity += item.Quantity;

        }

        // Getting subtotal
        decimal subTotal = await CalculateSubTotalAsync(order, ct);

        decimal shippingCost = 0m;
        if (subTotal < 1000)
        {
            shippingCost = 5 + 1 * totalQuantity + 2 * totalWeight;
        }

        return Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);

    }


}
