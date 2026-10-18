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

# Publish for macOS x64 (Intel)
echo "📦 Publishing for macOS x64..."
dotnet publish LifTools/LifTools.csproj \
    --configuration Release \
    --runtime osx-x64 \
    --self-contained true \
    --output dist/osx-x64/ \
    --property:PublishSingleFile=true

# Publish for macOS arm64 (Apple Silicon)
echo "📦 Publishing for macOS arm64..."
dotnet publish LifTools/LifTools.csproj \
    --configuration Release \
    --runtime osx-arm64 \
    --self-contained true \
    --output dist/osx-arm64/ \
    --property:PublishSingleFile=true

# Copy VERSION.txt to distributions if it exists
if [ -f "VERSION.txt" ]; then
    echo "📋 Copying VERSION.txt to distributions..."
    cp VERSION.txt dist/win-x64/
    cp VERSION.txt dist/osx-x64/
    cp VERSION.txt dist/osx-arm64/
fi

# Wrap a dotnet publish folder in an unsigned .app bundle.
# macOS only requires this directory layout; Developer ID signing is separate.
package_macos_app() {
    local runtime="$1"
    local publish_dir="dist/${runtime}"
    local app_root="${publish_dir}/LifTools.app"
    local contents="${app_root}/Contents"
    local macos="${contents}/MacOS"
    local version="1.0.0"

    if [ -f "VERSION.txt" ]; then
        version=$(tr -d '[:space:]' < VERSION.txt)
    fi

    echo "📦 Packaging ${runtime} as LifTools.app..."
    rm -rf "$app_root"
    mkdir -p "$macos"

    shopt -s nullglob
    for file in "$publish_dir"/*; do
        local base
        base=$(basename "$file")
        case "$base" in
            *.pdb|*.xml|LifTools.app) continue ;;
        esac
        if [ -f "$file" ]; then
            mv "$file" "$macos/"
        fi
    done
    shopt -u nullglob

    chmod +x "${macos}/LifTools"

    cat > "${contents}/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>LifTools</string>
  <key>CFBundleDisplayName</key>
  <string>LifTools</string>
  <key>CFBundleIdentifier</key>
  <string>com.speedskating.liftools</string>
  <key>CFBundleVersion</key>
  <string>${version}</string>
  <key>CFBundleShortVersionString</key>
  <string>${version}</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleSignature</key>
  <string>????</string>
  <key>CFBundleExecutable</key>
  <string>LifTools</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>LSMinimumSystemVersion</key>
  <string>11.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
EOF
}

package_macos_app osx-x64
package_macos_app osx-arm64

# Create Windows distribution zip
echo "📁 Creating Windows distribution zip..."
cd dist/win-x64/
zip -r ../../LifTools-win-x64.zip . -x "*.pdb" "*.xml"
cd ../..

# Create macOS distribution zips (the .app is the only payload)
echo "📁 Creating macOS distribution zips..."
cd dist/osx-x64/
zip -r -y ../../LifTools-macos-x64.zip LifTools.app -x "*.pdb" "*.xml"
cd ../..
cd dist/osx-arm64/
zip -r -y ../../LifTools-macos-arm64.zip LifTools.app -x "*.pdb" "*.xml"
cd ../..

# Display results
echo ""
echo "✅ Build completed successfully!"
echo ""
echo "📦 Distribution files created:"
echo "   - LifTools-win-x64.zip"
echo "   - LifTools-macos-x64.zip"
echo "   - LifTools-macos-arm64.zip"
echo ""
echo "📊 File sizes:"
ls -lh LifTools-*.zip
echo ""
echo "🎉 Ready for distribution!"
