## Phase 1 questions

### 1- Why are the sync and async times almost the same when orders are awaited one after another?

await only stops the current order until the timer ended and foreach doesn't go with the next order until finishing the current so the two operations almost wait the same but the difference is in the save operation that we used in async part which is 20 sec and that is the actual difference as shown form result :

Sync Time: 34.9 seconds
Async Time: 54.6 seconds

#### 2- Is the thread ID before an await always the same as after it? Why? Use your own log lines as evidence.

No, because await back thread to thread pool and not guarantee to use the same thread after continuation because console applications don't contain SyncronizationContext to return the same thread back

Sync evidence
[08:34:55.619] [Thread 9] Phase1Runner -> = Sync run: validating the orders one by one =
[08:34:55.620] [Thread 9] OrderValidator -> Validating Order #1...
[08:34:55.655] [Thread 9] customers.json -> Open Connection . .
[08:34:55.870] [Thread 9] CustomerRepository -> Executing query for Customer #1...
[08:34:56.239] [Thread 9] CustomerRepository -> Customer #1 found (Ahmed Ali)
[08:34:56.240] [Thread 9] products.json -> Open Connection . .
[08:34:56.449] [Thread 9] ProductRepository -> Executing query for Product #1...
[08:34:56.938] [Thread 9] ProductRepository -> Product #1 found (Laptop) stock 10
[08:34:56.938] [Thread 9] products.json -> Open Connection . .
[08:34:57.153] [Thread 9] ProductRepository -> Executing query for Product #3...
[08:34:57.679] [Thread 9] ProductRepository -> Product #3 found (Wireless Mouse) stock 2
[08:34:57.680] [Thread 9] coupons.json -> Open Connection . .
[08:34:57.889] [Thread 9] CouponRepository -> Executing query for Coupon of code #SAVE10...
[08:34:58.196] [Thread 9] CustomerRepository -> Coupon #SAVE10 found (10%)
[08:34:58.196] [Thread 9] OrderValidator -> Order #1 validated

Async evidence
[08:35:30.637] [Thread 9] Phase1Runner -> = Async run: validating the orders one by one =
[08:35:30.639] [Thread 9] OrderValidator -> Validating Order #1...
[08:35:30.640] [Thread 9] customers.json -> Open Connection . .
[08:35:30.854] [Thread 9] CustomerRepository -> Executing query for Customer #1...
[08:35:31.258] [Thread 12] CustomerRepository -> Customer #1 found (Ahmed Ali)
[08:35:31.259] [Thread 12] products.json -> Open Connection . .
[08:35:31.467] [Thread 12] ProductRepository -> Executing query for Product #1...

#### 3- What does the thread do while Task.Delay() is waiting? And while Thread.Sleep() is waiting?

while using Task.delay => the thread is free and backed to pool so it can do something else
while Thread.sleep => the thread is blocked and do nothing until the timer is ended

#### 4- Why should a repository method return Task instead of T?

because Task is a promise for a value so when the method reach await statement it returning unfinished task to the caller so caller could use await to wait the task until be finished without blocking the thread and also Task has status and exception and there is another advantage which is the caller can execute many tasks at the same period and wait them all later to achieve concurrency . If we used T the caller will block the thread until method finished

### 5- What is the difference between File.ReadAllText and File.ReadAllTextAsync? Does the async

version occupy a thread while the disk is reading?

The two are an i/o bound operation but the difference is that while reading file the thread is blocked and do nothing with readAllText which is not happening in the ReadAllTextAsync.
