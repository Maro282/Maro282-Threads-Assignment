# Assignment 02 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

## Recorded run results

Although we have TablesLeft=5 i noticed that :-

- First run we got Booked=13, TablesLeft=-8
- second run we got Booked=19, TablesLeft=-26
- third run we got Booked=35, TablesLeft=-30

## What is wrong

- Absolutely from the observed code and results there is a race condition happening because there is no handle for the remaining tables left on Friday Booking class

- we are using thread.sleep() which block thread

- we are creating threads by hand in program.cs

## How you fixed it

- for handling the race condition we used semaphoreslim with initial 1 thread allowed and max = 1 to allow only 1 thread to perform task at the time

- another reason for using semaphoreslim is to manage us using await inside it to change thread.sleep() to task.dealy() to lont block the the thread at this moment of waiting something happened

- in the program.cs we used a list of tasks instead of creating threads with our hands then used whenAll to wait all tasks finished first instead of thread.join() which blocks thread then display the final result of booked and left tables
