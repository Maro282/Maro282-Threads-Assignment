# Assignment 04 — Answers

Student name: Marwan Abdelhamied Hassan
Group / ID: G4

## Why Expected and Actual disagree

- because the operation of Views++ is not atomic. Actually it devided into 3 steps :-
  1- Read
  2- add
  3- write
  So threads for sure will be Intertwined while processing the code . it will cause data lost.

## What you changed

- i have made the operation on incrementing the views atomic so no thread could do anything while another thread is working

## When would you protect the update differently

- i will use another methodology like lock if the operation containing :-
- If condition
- more than one variable wanted to be updated
- or the operation is more complex than incrementing one variable
