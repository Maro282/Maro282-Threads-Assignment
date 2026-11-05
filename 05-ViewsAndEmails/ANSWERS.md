# Assignment 05 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

## Failures you observed

- i found from running the program several times that The expected result numbers were not same as actual and also in many other times tha app throws and that was expected from this unhandled multi threading code because dictionary and queue designed for only one thread

## What you changed

- i have changed the collection from regular to concurrent :-
  1- concurrentDictionary handle multi threading and allow only one atomic operation done at a time so no other threads could do anything while one is already processing . and also ConcurrentDictionary prevent the exception that could be happened when two threads trying to add item with the same name at the same time.

  2- ConcurrentQueue handle the exception that could be happend if threads try to dequeue the only last item in the queue so, no thread will dequeue a null .

## Collection safety vs business-rule safety

- making the collection thread safe doesn't mean to make the bussiness rule thread safe. so, using ConcurrentDictionary and ConcurrentQueue for example doesn't make the bussiness rule thread safe. because these thread safe collections make it's data safe againist concurrent workers but if we have multiple related steps to serve the bussiness rule like (check availabilty and then update) the concurrent collection itself do not garantue to protect bussiness rule so we should use lock or something like that to protect the bussiness rule
