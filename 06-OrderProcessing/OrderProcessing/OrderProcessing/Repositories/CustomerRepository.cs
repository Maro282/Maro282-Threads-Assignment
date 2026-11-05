
using OrderProcessing.Exceptions;
using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class CustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
    {
        // Simulating connection with dataBase
        Log.Info(nameof(CustomerRepository), "Open Connection . . ");
        await Task.Delay(200, ct);

        // Simulation processing a Query
        Log.Info(nameof(CustomerRepository), $"Executing query for Customer #{id}...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);

        //PROCESS
        List<Customer> customers = await JsonFileStore.ReadFromJsonFileASync<Customer>(DataFiles.customers, ct);
        Customer? result = customers.FirstOrDefault(i => i.Id == id);
        Log.Info(nameof(CustomerRepository),
                 result is null ? $"Customer #{id} not found"
                 : $"Customer #{id} found ({result.Name})");

        //RESULT
        return result;

    }

}
