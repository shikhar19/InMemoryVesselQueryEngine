# In-Memory Vessel Query Engine

## Overview
This project implements a basic in-memory database using C#.

It loads vessel data from a JSON file and allows dynamic querying using a simplified WHERE clause syntax.

## Supported Query Features

- Numeric comparisons: =, <, >
- Text equality comparison
- Multiple conditions using AND

## Example Queries

WHERE Z13_STATUS_CODE = 4  
WHERE Z13_STATUS_CODE < 4  
WHERE BUILDER_GROUP = 'Guoyu Logistics'  
WHERE Z13_STATUS_CODE = 4 AND BUILDER_GROUP = 'Guoyu Logistics'

## How to Run

1. Open solution in Visual Studio
2. Build and run
3. Enter query in console

## Notes
- No database engine or external libraries were used.
- All filtering logic is implemented manually in C#.
