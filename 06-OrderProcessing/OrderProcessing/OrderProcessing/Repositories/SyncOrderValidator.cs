

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class SyncOrderValidator
{

    private readonly string Name = nameof(OrderValidator);
    private readonly SyncCouponRepository _couponRepository;
    private readonly SyncCustomerRepository _customerRepository;
    private readonly SyncProductRepository _productRepository;

    public SyncOrderValidator(SyncCustomerRepository customerRepository, SyncProductRepository productRepository, SyncCouponRepository couponRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
    }

    public bool ValidateOrder(Order order)
    {

        Log.Info(Name, $"Validating Order #{order.Id}...");

        // 1- Check that customer exists

        Customer? customer = _customerRepository.GetById(order.CustomerId);
        if (customer == null)
        {
            order.Status = OrderStatus.Rejected;
            Log.Error(Name, $"Order #{order.Id} rejected: Customer id not found");
            return false;

        }

        // 2- check customer is active or not
        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            Log.Error(Name, $"Order #{order.Id} rejected: Customer is inactive");
            return false;
        }

        // Check that all products found
        List<Product> products = new();
        foreach (OrderItem item in order.Items)
        {
            Product? product = _productRepository.GetById(item.ProductId);
            if (product is null)
            {
                Log.Error(Name, $"Order #{order.Id} rejected: Product : {product?.Id} not found");
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
                return false;
            }
        }


        // Check that coupon is exist and valid


        if (order.CouponCode is not null)
        {
            Coupon? coupon = _couponRepository.GetByCode(order.CouponCode);
            if (coupon == null)
            {
                order.Status = OrderStatus.Rejected;
                Log.Error(Name, $"Order #{order.Id} rejected: Coupon {order.CouponCode} not found");
                return false;

            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                Log.Error(Name, $"Order #{order.Id} rejected: Coupon {order.CouponCode} Expired");
                return false;

            }
        }

        order.Status = OrderStatus.Validated;
        Log.Success(Name, $"Order #{order.Id} validated");
        return true;



    }
}
