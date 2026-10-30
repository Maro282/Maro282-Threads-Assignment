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

-

## When would you protect the update differently

-
