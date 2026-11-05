using OrderProcessing;
using OrderProcessing.Exceptions;
using OrderProcessing.Models;
using OrderProcessing.Repositories;
using OrderProcessing.Runners;
using OrderProcessing.Services;


CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken ct = cts.Token;

// Phase 1 runner test
var runner = new Phase1Runner(
    new IncomingOrderRepository(),
    new OrderRepository(),
    new OrderValidator(new CustomerRepository(), new ProductRepository(), new CouponRepository()),
    new SyncOrderValidator(new SyncCustomerRepository(), new SyncProductRepository(), new SyncCouponRepository()));

//await runner.RunAsync(CancellationToken.None);



// Test unsafe reservation 
var productRepository = new ProductRepository();
var raceConditionDemo =
    new RaceConditionDemo(productRepository);

//await raceConditionDemo.RunAsync(ct);


// phase 2 Runner test

// 1. Create Repositories
var incomingOrderRepository = new IncomingOrderRepository();
var orderRepository = new OrderRepository();
//var productRepository = new ProductRepository();

// 2. Create Services
var customerRepository = new CustomerRepository();
var couponRepository = new CouponRepository();

var pricingService = new PricingService(
    customerRepository,
    couponRepository,
    new[] { 7 });

var orderPricer = new OrderPricer(pricingService);

var invoiceGenerator = new InvoiceGenerator();

var orderValidator = new OrderValidator(
    customerRepository,
    productRepository,
    couponRepository);

// 3. Create Order Processor
var orderProcessor = new OrderProcessorV2(
    productRepository,
    orderRepository,
    orderPricer,
    invoiceGenerator);

// 4. Create Report Generator
var reportGenerator = new ReportGenerator(
    productRepository,
    orderRepository);

// 5. Create Phase2Runner
var phase2Runner = new Phase2Runner(
    new RaceConditionDemo(productRepository),
    orderRepository,
    incomingOrderRepository,
    orderPricer,
    orderValidator,
    orderProcessor,
    productRepository,
    reportGenerator);

// 6. Run Phase 2
//await phase2Runner.RunAsync(ct);




// Test 9.1 


var incoming = await new IncomingOrderRepository().GetAllAsync(ct);

// 1) CustomerService
var customers = new CustomerService(new CustomerRepository());
Customer omar = await customers.GetCustomerAsync(3, ct);
try { await customers.GetCustomerAsync(99, ct); }
catch (CustomerNotFoundException ex) { Console.WriteLine(ex.Message); }

// 2) InventoryService: الكل أو لا شيء
var inventory = new InventoryService(new ProductRepository());
var test = new Order
{
    Id = 999,
    Items = new List<OrderItem>
{
    new() { ProductId = 1, Quantity = 1 },      // Laptop: متوفر
    new() { ProductId = 3, Quantity = 5 }       // Mouse: مخزونه 2، هيفشل
}
};
Console.WriteLine(await inventory.ReserveStockAsync(test, ct));   // false
Console.WriteLine(test.FailureReason);                            // Insufficient stock for 'Wireless Mouse'

// 3) PricingService (بدون shipping failures)
var pricing = new PricingService(new CustomerRepository(), new CouponRepository(), Array.Empty<int>());
Console.WriteLine(await pricing.CalculateTotalAsync(incoming[0], ct));    // 980.00
Console.WriteLine(await pricing.CalculateTotalAsync(incoming[1], ct));    // 767.30
try { await pricing.CalculateTotalAsync(incoming[11], ct); }              // order #12
catch (InvalidCouponException ex) { Console.WriteLine(ex.Message); }      // Coupon 'OLD20' has expired

// 4) PaymentGateway
var gateway = new PaymentGateway();
PaymentResult result = await gateway.ProcessPaymentAsync(2, 767.30m, ct);
Console.WriteLine(result.TransactionId);          // TXN-XXXXXXXXXXXX (12 حرف كبيرة)
await gateway.RefundAsync(result.TransactionId!, ct);

// 5) EmailService
await new EmailService().SendConfirmationAsync(incoming[1], omar, ct);