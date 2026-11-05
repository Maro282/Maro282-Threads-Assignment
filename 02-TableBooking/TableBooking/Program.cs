using TableBooking;

var booking = new FridayNightBooking();
//Thread[] customers = new Thread[40];
List<Task> tasks = new List<Task>();

for (int i = 0; i < 40; i++)
{
    string name = $"Customer-{i + 1:00}";
    tasks.Add(Task.Run(() => booking.Book(name)));
}

//foreach (Thread t in customers)
//    t.Join();

await Task.WhenAll(tasks);

Console.WriteLine();
Console.WriteLine($"Booked={booking.Booked}, TablesLeft={booking.TablesLeft}");
