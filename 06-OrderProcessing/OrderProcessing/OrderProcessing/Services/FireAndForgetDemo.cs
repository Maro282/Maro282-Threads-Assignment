
using OrderProcessing.Interfaces;
using OrderProcessing.Models;
using OrderProcessing.Repositories;

namespace OrderProcessing.Services;

public class FireAndForgetDemo
{
    private readonly OrderRepository _orderRepository;
    private readonly IEmailService _emailService;

    public FireAndForgetDemo(
        OrderRepository orderRepository,
        IEmailService emailService)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
    }

    // BROKEN VERSION — Task 9.7
    public async Task ProcessOrderAsync(
        Order order,
        Customer customer, CancellationToken ct = default)
    {
        await _orderRepository.SaveAsync(order, ct);

#pragma warning disable CS4014
        _emailService.SendConfirmationAsync(order, customer);
#pragma warning restore CS4014

        Console.WriteLine("Order processing finished");
    }

    // FIXED VERSION
    public async Task ProcessOrderFixedAsync(
        Order order,
        Customer customer)
    {
        await _orderRepository.SaveAsync(order);

        await _emailService.SendConfirmationAsync(
            order, customer);

        Console.WriteLine("Order processing finished");
    }
}
