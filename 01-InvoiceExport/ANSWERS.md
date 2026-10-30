# Assignment 01 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

## Problems with the current design

- I have noticed that the desgin using thread.sleep() almost in every Method to simulate I/O bound timing but this approach blocking the thread until finish which is not the best practice of using thread with I/O bound operations

- in program.cs while creating the path of invoice Dir we depend on CWD which could be changed based on the way we run the program with

- in program.cs the function SeedCustomerFiles write in files so we should not block the thread while finishing we need also to modify this function to non block the thread while writing

- another problem which in OrderDataBase file we returning a list of order which is not protected againist update or modification outside the class so we should return a readonly list of orders

## What you changed

-
