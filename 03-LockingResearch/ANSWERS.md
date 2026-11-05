# Assignment 03 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

1. used to control access to shared resources between multiple threads (preventing race condition)

2. it's complie to

   bool lockWasTaken = false;
   var temp = obj;
   try
   {
   Monitor.Enter(temp, ref lockWasTaken);
   // body
   }
   finally
   {
   if (lockWasTaken)
   {
   Monitor.Exit(temp);
   }
   }

3. - lock is a sugar syntax of using Monitor class but Monitor class offers more flexibility, allowing you to use other methods like Monitor.Wait and Monitor.Pulse for more advanced synchronization scenarios.

   - also note that in recent versions of C# the Monitor class is used internally by the lock statement.

4. we use these monitor methods when we need more control on the thread

- monitor.tryEnter() => we used it when we want the thread to try acquire a lock without waiting indefinitely so we specify a time if the lock isn't released then go do something else.

- monitor.wait() => when we want a thread temporarily release a lock and wait until another thread signals it.

- monitor.pulse() => we use it when we want to notify the waiting thread to be ready for the locked obj because it will be released . After the release is done the waiting thread reacquires the lock

5.  take a look in the photo

Links:

- https://learn.microsoft.com/en-us/dotnet/api/system.threading.monitor?view=net-10.0
- https://stackoverflow.com/questions/6029804/what-does-a-lock-statement-do-under-the-hood
- https://zetcode.com/csharp/monitor/
- https://medium.com/@kamlesh90/lock-vs-monitor-in-c-a16560c49a23
