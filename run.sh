#!/bin/bash

# Build and run the LifTools application
echo "Building LifTools..."
dotnet build

if [ $? -eq 0 ]; then
    echo "Build successful. Running LifTools..."
    dotnet run --project LifTools/LifTools.csproj
else
    echo "Build failed. Please check the errors above."
    exit 1
fi
