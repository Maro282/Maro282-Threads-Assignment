using OrderProcessing.Exceptions;
using OrderProcessing.Models;
using OrderProcessing.Repositories;

namespace OrderProcessing.Services;

public class CustomerService
{

    private readonly CustomerRepository _customerRepository;

    public CustomerService(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> GetCustomerAsync(int id, CancellationToken ct)
    {

        Customer? customer = await _customerRepository.GetByIdAsync(id, ct);

        if (customer is null) throw new CustomerNotFoundException($"Customer #{id} not found");

        return customer;
    }



}

