---
title: Memory Management Notes
date: 2025.01.31
---
- Casey Muratori
    - Up-front allocation
    - Arenas?
- Ryan Fluery
    - Possible to dynamically allocate on the stack
    - `alloca` 
    - Arena allocators are strictly better
    - "There's no true dichotomy between stack and heap allocation. The stack is just one dynamic allocation that the operating system has done for you"
    - "The language is not the problem, you need to use the tools better"
- .NET Memory Management Chapter 2
    - DDR/Memory speeds
    - Admit that wasn't able to reproduce these
    - tCL/tRCD
    - "The most influential fact is that the data between the RAM and the cache is transfered in blocks called "cache line"
    - Column vs. row indexing
    - Hierarchical cache example
    - Multicore cache example

