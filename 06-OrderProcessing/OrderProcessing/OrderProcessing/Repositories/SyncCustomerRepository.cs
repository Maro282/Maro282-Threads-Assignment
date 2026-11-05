

using OrderProcessing.Models;

namespace OrderProcessing.Repositories;

public class SyncCustomerRepository
{

    public Customer? GetById(int id)
    {
        // Simulating connection with dataBase
        Log.Info(DataFiles.customers, "Open Connection . . ");
        Thread.Sleep(200);

        // Simulation processing a Query
        Log.Info(nameof(CustomerRepository), $"Executing query for Customer #{id}...");
        Thread.Sleep(Random.Shared.Next(300, 601));

        //PROCESS
        List<Customer> customers = JsonFileStore.ReadFromJsonFile<Customer>(DataFiles.customers);
        Customer? result = customers.FirstOrDefault(i => i.Id == id);
        Log.Info(nameof(CustomerRepository),
                 result is null ? $"Customer #{id} not found"
                 : $"Customer #{id} found ({result.Name})");

        //RESULT
        return result;

    }
}
