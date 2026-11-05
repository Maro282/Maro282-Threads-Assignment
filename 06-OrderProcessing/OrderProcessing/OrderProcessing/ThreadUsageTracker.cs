
using System.Collections.Concurrent;

namespace OrderProcessing.Shared;

public static class ThreadUsageTracker
{
    private static readonly ConcurrentDictionary<int, byte>
        ThreadIds = new();

    public static void RecordCurrentThread()
    {
        int threadId = Environment.CurrentManagedThreadId;
        ThreadIds.TryAdd(threadId, 0);
    }

    public static int Count => ThreadIds.Count;

    public static void Reset()
    {
        ThreadIds.Clear();
    }
}
