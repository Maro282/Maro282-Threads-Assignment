namespace TableBooking;

public sealed class FridayNightBooking
{

    private readonly SemaphoreSlim _reservation = new SemaphoreSlim(1, 1);
    public int TablesLeft { get; private set; } = 5;
    public int Booked { get; private set; }

    public async Task Book(string customerName)
    {
        await _reservation.WaitAsync();

        try
        {

            if (TablesLeft <= 0)
                return;

            await Task.Delay(20);

            TablesLeft--;
            Booked++;
            Console.WriteLine($"{customerName} booked a table. Left={TablesLeft}");
        }
        finally { _reservation.Release(); }
    }
}
