#!/bin/bash

# Create output directory for all platforms
DIST_DIR="dist"
rm -rf $DIST_DIR
mkdir -p $DIST_DIR

# Windows (x64)
dotnet publish -c Release -r win-x64 --self-contained true
cp bin/Release/net8.0/win-x64/publish/mnpq.exe $DIST_DIR/mnpq.win.x64.exe

# macOS (x64)
dotnet publish -c Release -r osx-x64 --self-contained true
cp bin/Release/net8.0/osx-x64/publish/mnpq $DIST_DIR/mnpq.osx.x64

# macOS (ARM64 - Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true
cp bin/Release/net8.0/osx-arm64/publish/mnpq $DIST_DIR/mnpq.osx.arm64

# Linux (x64)
dotnet publish -c Release -r linux-x64 --self-contained true
cp bin/Release/net8.0/linux-x64/publish/mnpq $DIST_DIR/mnpq.linux.x64

# Linux (ARM64)
dotnet publish -c Release -r linux-arm64 --self-contained true
cp bin/Release/net8.0/linux-arm64/publish/mnpq $DIST_DIR/mnpq.linux.arm64