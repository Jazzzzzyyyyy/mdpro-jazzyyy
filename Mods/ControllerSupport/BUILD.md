# Building ControllerSupport.dll

This guide explains how to build the Controller Support mod as a DLL for MDPro3.

## Prerequisites

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download) or Visual Studio 2022
- MDPro3 game installation (for reference assemblies)

## Build Steps

### Option 1: Using .NET CLI (Recommended)

1. Open a terminal/command prompt in this folder (`Mods/ControllerSupport/`)

2. Set the GamePath to your MDPro3 installation:
   ```bash
   # Windows
   set GamePath=C:\Games\MDPro3\
   
   # Linux/macOS
   export GamePath=/path/to/MDPro3/
   ```

3. Build the DLL:
   ```bash
   dotnet build -c Release
   ```

4. The compiled DLL will be in `bin/Release/netstandard2.1/ControllerSupport.dll`

### Option 2: Using Visual Studio

1. Open `ControllerSupport.csproj` in Visual Studio 2022
2. Edit the `GamePath` property in the .csproj file to point to your MDPro3 installation
3. Build the solution (Ctrl+Shift+B) in Release mode
4. Find the DLL in `bin/Release/netstandard2.1/`

## Troubleshooting Build Issues

### Missing Reference Assemblies

If you get errors about missing references, ensure:
- `GamePath` points to your MDPro3 installation root folder
- The `MDPro3_Data/Managed/` folder exists with the game's DLL files
- You have the required Unity assemblies (UnityEngine.dll, Unity.InputSystem.dll, etc.)

### Wrong .NET Version

The project targets `netstandard2.1` for Unity compatibility. If you have issues:
- Ensure you have .NET SDK 6.0 or later installed
- Check with `dotnet --version`

## Output Files

After a successful build, you'll have:
- `ControllerSupport.dll` - The main mod DLL

## Installation

See [INSTALL.md](INSTALL.md) for instructions on installing the built DLL into MDPro3.
