# Assignment 01 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

## Problems with the current design

- I have noticed that the desgin using thread.sleep() almost in every Method to simulate I/O bound timing but this approach blocking the thread until finish which is not the best practice of using thread with I/O bound operations

- another problem which in OrderDataBase file we returning a list of order which is not protected againist update or modification outside the class so we should return a readonly list of orders

## What you changed

-
