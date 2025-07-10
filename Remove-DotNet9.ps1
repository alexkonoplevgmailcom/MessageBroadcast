# .NET 9 Removal Script
# This script removes all .NET 9 components from your system
# Run this script as Administrator

Write-Host "Starting .NET 9 removal process..." -ForegroundColor Yellow

# Get all .NET 9 related products
$dotnet9Products = Get-WmiObject -Class Win32_Product | Where-Object { 
    $_.Name -like "*NET*9*" -or 
    $_.Name -like "Microsoft ASP.NET Core 9*" -or
    $_.Name -like "Microsoft .NET*9*"
}

Write-Host "Found $($dotnet9Products.Count) .NET 9 components to remove:" -ForegroundColor Cyan
foreach ($product in $dotnet9Products) {
    Write-Host "  - $($product.Name) ($($product.Version))" -ForegroundColor Gray
}

Write-Host "`nRemoving .NET 9 components..." -ForegroundColor Yellow

foreach ($product in $dotnet9Products) {
    try {
        Write-Host "Removing: $($product.Name)..." -ForegroundColor Green
        $result = $product.Uninstall()
        if ($result.ReturnValue -eq 0) {
            Write-Host "  ✓ Successfully removed" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Failed to remove (Return code: $($result.ReturnValue))" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
    }
}

# Also try to remove SDK using the uninstall tool
$uninstallTool = "C:\Program Files (x86)\dotnet-core-uninstall\dotnet-core-uninstall.exe"
if (Test-Path $uninstallTool) {
    Write-Host "`nTrying to remove .NET 9 SDK using uninstall tool..." -ForegroundColor Yellow
    try {
        & $uninstallTool remove --sdk 9.0.301 --force --yes
        Write-Host "  ✓ SDK removal completed" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ SDK removal failed: $_" -ForegroundColor Red
    }
}

# Clean up any remaining directories
$dotnetPath = "C:\Program Files\dotnet"
$sharedPath = "$dotnetPath\shared"

Write-Host "`nCleaning up remaining .NET 9 directories..." -ForegroundColor Yellow

# Remove .NET 9 SDK directories
$sdkPath = "$dotnetPath\sdk"
if (Test-Path $sdkPath) {
    Get-ChildItem $sdkPath -Directory | Where-Object { $_.Name -like "9.*" } | ForEach-Object {
        try {
            Write-Host "Removing SDK directory: $($_.FullName)" -ForegroundColor Green
            Remove-Item $_.FullName -Recurse -Force
            Write-Host "  ✓ Removed" -ForegroundColor Green
        }
        catch {
            Write-Host "  ✗ Failed to remove: $_" -ForegroundColor Red
        }
    }
}

# Remove .NET 9 runtime directories
if (Test-Path $sharedPath) {
    Get-ChildItem $sharedPath -Directory | ForEach-Object {
        $runtimePath = $_.FullName
        Get-ChildItem $runtimePath -Directory | Where-Object { $_.Name -like "9.*" } | ForEach-Object {
            try {
                Write-Host "Removing runtime directory: $($_.FullName)" -ForegroundColor Green
                Remove-Item $_.FullName -Recurse -Force
                Write-Host "  ✓ Removed" -ForegroundColor Green
            }
            catch {
                Write-Host "  ✗ Failed to remove: $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host "`n=== .NET 9 Removal Complete ===" -ForegroundColor Cyan
Write-Host "Verifying remaining .NET installations..." -ForegroundColor Yellow

# Verify what's left
Write-Host "`nRemaining .NET SDKs:" -ForegroundColor Cyan
& dotnet --list-sdks

Write-Host "`nRemaining .NET Runtimes:" -ForegroundColor Cyan
& dotnet --list-runtimes

Write-Host "`nIf you see any .NET 9 versions still listed above, you may need to:" -ForegroundColor Yellow
Write-Host "1. Restart your computer" -ForegroundColor Gray
Write-Host "2. Check Visual Studio Installer for .NET 9 components" -ForegroundColor Gray
Write-Host "3. Manually remove from Programs and Features" -ForegroundColor Gray

Write-Host "`nScript completed. Press any key to continue..." -ForegroundColor Green
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
