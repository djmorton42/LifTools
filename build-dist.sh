#!/bin/bash

# Build and distribution script for LifTools
# Creates self-contained executables for Windows and macOS

set -e  # Exit on any error

echo "🚀 Building LifTools distribution packages..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean
rm -rf dist/
mkdir -p dist/

# Build the project first to ensure everything compiles
echo "🔨 Building project..."
dotnet build --configuration Release

# Publish for Windows x64
echo "📦 Publishing for Windows x64..."
dotnet publish LifTools/LifTools.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output dist/win-x64/ \
    --property:PublishSingleFile=true

# Publish for macOS x64
echo "📦 Publishing for macOS x64..."
dotnet publish LifTools/LifTools.csproj \
    --configuration Release \
    --runtime osx-x64 \
    --self-contained true \
    --output dist/osx-x64/ \
    --property:PublishSingleFile=true

# Copy VERSION.txt to both distributions if it exists
if [ -f "VERSION.txt" ]; then
    echo "📋 Copying VERSION.txt to distributions..."
    cp VERSION.txt dist/win-x64/
    cp VERSION.txt dist/osx-x64/
fi

# Create Windows distribution zip
echo "📁 Creating Windows distribution zip..."
cd dist/win-x64/
zip -r ../../LifTools-win-x64.zip . -x "*.pdb" "*.xml"
cd ../..

# Create macOS distribution zip
echo "📁 Creating macOS distribution zip..."
cd dist/osx-x64/
zip -r ../../LifTools-macos-x64.zip . -x "*.pdb" "*.xml"
cd ../..

# Display results
echo ""
echo "✅ Build completed successfully!"
echo ""
echo "📦 Distribution files created:"
echo "   - LifTools-win-x64.zip"
echo "   - LifTools-macos-x64.zip"
echo ""
echo "📊 File sizes:"
ls -lh LifTools-*.zip
echo ""
echo "🎉 Ready for distribution!"
