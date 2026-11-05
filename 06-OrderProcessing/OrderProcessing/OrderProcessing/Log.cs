

namespace OrderProcessing;

public static class Log
{

    private static readonly Lock _lock = new();

    public static void Info(string source, string message)
    {
        WriteLine(source, message, ConsoleColor.Gray);
    }
    public static void Success(string source, string message)
    {
        WriteLine(source, message, ConsoleColor.Green);
    }
    public static void Error(string source, string message)
    {
        WriteLine(source, message, ConsoleColor.Red);
    }


    private static void WriteLine(string source, string message, ConsoleColor color)
    {
        lock (_lock)
        {


            Console.ForegroundColor = color;

            Console.WriteLine(
                $"[{DateTime.Now:HH:mm:ss.fff}] " +
                $"[Thread {Environment.CurrentManagedThreadId}] " +
                $"{source} -> {message}"
                );

            Console.ResetColor();

        }
    }




}
