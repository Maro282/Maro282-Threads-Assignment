

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class OrderValidator
{

    private readonly string Name = nameof(OrderValidator);

    private readonly CouponRepository _couponRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly ProductRepository _productRepository;

    public OrderValidator(CustomerRepository customerRepository, ProductRepository productRepository, CouponRepository couponRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
    }

    public async Task<bool> ValidateOrderAsync(Order order, CancellationToken ct)
    {

        Log.Info(Name, $"Validating Order #{order.Id}...");

        // 1- Check that customer exists

        Customer? customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);
        if (customer == null)
        {
            order.Status = OrderStatus.Rejected;
            Log.Error(Name, $"Order #{order.Id} rejected: Customer id not found");
            order.FailureReason = "Customer id not found";
            return false;

        }

        // 2- check customer is active or not
        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            Log.Error(Name, $"Order #{order.Id} rejected: Customer is inactive");
            order.FailureReason = " Customer is inactive";
            return false;
        }

        // Check that all products found
        List<Product> products = new();
        foreach (OrderItem item in order.Items)
        {
            Product? product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product is null)
            {
                Log.Error(Name, $"Order #{order.Id} rejected: Product : {product?.Id} not found");

                order.FailureReason = $"Product : {product?.Id} not found";
                return false;
            }
            products.Add(product!);
        }

        // Check each product quantity > product quantity in stock
        for (int i = 0; i < products.Count; i++)
        {
            if (products[0].StockQuantity < order.Items[0].Quantity)
            {
                Log.Error(Name, $"Order #{order.Id} rejected: Insufficient stock for '{products[i].Name}'");
                order.FailureReason = $"Insufficient stock for '{products[i].Name}'";

                return false;
            }
        }


        // Check that coupon is exist and valid


        if (order.CouponCode is not null)
        {
            Coupon? coupon = await _couponRepository.GetByCodeAsync(order.CouponCode, ct);
            if (coupon == null)
            {
                order.Status = OrderStatus.Rejected;
                Log.Error(Name, $"Order #{order.Id} rejected: Coupon {order.CouponCode} not found");
                order.FailureReason = $"Coupon {order.CouponCode} not found";

                return false;

            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                Log.Error(Name, $"Order #{order.Id} rejected: Coupon {order.CouponCode} Expired");
                order.FailureReason = $"Coupon {order.CouponCode} Expired";
                return false;

            }
        }

        order.Status = OrderStatus.Validated;
        Log.Success(Name, $"Order #{order.Id} validated");
        return true;




    }


}
