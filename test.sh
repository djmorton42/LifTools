#!/bin/bash

# Run tests for the LifTools application
echo "Running LifTools tests..."
dotnet test

if [ $? -eq 0 ]; then
    echo "All tests passed! ✅"
else
    echo "Some tests failed. Please check the output above. ❌"
    exit 1
fi
