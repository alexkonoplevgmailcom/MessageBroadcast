# How to Remove .NET 9 from Your PC

## Quick Instructions

### Option 1: Run the Automated Script (Recommended)
1. **Right-click on PowerShell** and select **"Run as Administrator"**
2. Navigate to your project directory:
   ```powershell
   cd "C:\Users\FIBI\Repos\alexkonoplevgmailcom\MessageBroadcast"
   ```
3. Run the removal script:
   ```powershell
   .\Remove-DotNet9-Complete.ps1
   ```

### Option 2: Manual Removal via Control Panel
1. Open **Control Panel** → **Programs and Features**
2. Find and uninstall ALL items containing ".NET 9" or "9.0":
   - Microsoft .NET SDK 9.0.301 (x64)
   - Microsoft .NET Runtime - 9.0.6 (x64)
   - Microsoft ASP.NET Core 9.0.6 Shared Framework (x64)
   - Microsoft .NET 9.0 Templates 9.0.301 (x64)
   - Microsoft .NET Toolset 9.0.301 (x64)
   - Microsoft .NET Host - 9.0.6 (x64)
   - Microsoft .NET Host FX Resolver - 9.0.6 (x64)
   - Microsoft .NET Targeting Pack - 9.0.6 (x64)
   - Microsoft ASP.NET Core 9.0.6 Targeting Pack (x64)
   - All Microsoft.NET.Sdk.*Manifest-9.0.100 entries
   - All Microsoft .NET AppHost Pack - 9.0.6 entries

### Option 3: Using winget (if available)
```powershell
winget uninstall Microsoft.DotNet.SDK.9
```

## What We Found Installed
The following .NET 9 components were detected on your system:
- Microsoft ASP.NET Core 9.0.6 Targeting Pack (x64)
- Microsoft .NET AppHost Pack - 9.0.6 (x64_x86)
- Microsoft .NET AppHost Pack - 9.0.6 (x64)
- Microsoft .NET Runtime - 9.0.6 (x64)
- Microsoft.NET.Sdk.MacCatalyst.Manifest-9.0.100 (x64)
- Microsoft.NET.Sdk.macOS.Manifest-9.0.100 (x64)
- Microsoft.NET.Sdk.tvOS.Manifest-9.0.100 (x64)
- Microsoft ASP.NET Core 9.0.6 Shared Framework (x64)
- Microsoft .NET Host FX Resolver - 9.0.6 (x64)
- Microsoft.NET.Sdk.Android.Manifest-9.0.100 (x64)
- Microsoft .NET 9.0 Templates 9.0.301 (x64)
- Microsoft .NET AppHost Pack - 9.0.6 (x64_arm64)
- Microsoft .NET Host - 9.0.6 (x64)
- Microsoft.NET.Sdk.iOS.Manifest-9.0.100 (x64)
- Microsoft.NET.Sdk.Maui.Manifest-9.0.100 (x64)
- Microsoft .NET Targeting Pack - 9.0.6 (x64)
- Microsoft .NET Toolset 9.0.301 (x64)

## Important Notes
- WARNING: Visual Studio may have installed .NET 9 - check Visual Studio Installer for .NET 9 workloads
- RESTART: You may need to restart your computer after removal
- SAFE: Your .NET 8 installation will remain intact
- CLEANUP: Manual directory cleanup may be needed in `C:\Program Files\dotnet\`

## After Removal
Your project will continue to work with .NET 8 as configured in:
- `global.json` (locks to .NET 8 SDK)
- `Directory.Build.props` (targets .NET 8)
- All `.csproj` files (target `net8.0`)

## Verification
After removal, verify .NET 9 is gone:
```powershell
dotnet --list-sdks
dotnet --list-runtimes
```

You should only see .NET 8.x versions listed.

## Troubleshooting
If .NET 9 components remain after removal:
1. Restart your computer
2. Check Visual Studio Installer for .NET 9 components
3. Manually delete directories in `C:\Program Files\dotnet\sdk\9.*`
4. Use the official .NET Uninstall Tool as Administrator
