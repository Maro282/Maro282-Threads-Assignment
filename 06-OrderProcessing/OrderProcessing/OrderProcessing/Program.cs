using OrderProcessing;
using OrderProcessing.Repositories;
using OrderProcessing.Runners;


// Phase 1 runner test
var runner = new Phase1Runner(
    new IncomingOrderRepository(),
    new OrderRepository(),
    new OrderValidator(new CustomerRepository(), new ProductRepository(), new CouponRepository()),
    new SyncOrderValidator(new SyncCustomerRepository(), new SyncProductRepository(), new SyncCouponRepository()));

await runner.RunAsync(CancellationToken.None);