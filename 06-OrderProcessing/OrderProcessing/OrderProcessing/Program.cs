using OrderProcessing;
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
await phase2Runner.RunAsync(ct);




