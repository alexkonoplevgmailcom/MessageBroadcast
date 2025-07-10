Write-Host "=== .NET 9 Complete Removal Script ===" -ForegroundColor Cyan
Write-Host "This script will remove ALL .NET 9 components from your system." -ForegroundColor Yellow
Write-Host ""

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click on PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Running as Administrator - proceeding with removal..." -ForegroundColor Green
Write-Host ""

# Step 1: Use the official uninstall tool for SDK
$uninstallTool = "C:\Program Files (x86)\dotnet-core-uninstall\dotnet-core-uninstall.exe"
if (Test-Path $uninstallTool) {
    Write-Host "Step 1: Removing .NET 9 SDK using official uninstall tool..." -ForegroundColor Yellow
    try {
        Write-Host "Executing: dotnet-core-uninstall remove --sdk 9.0.301 --force --yes" -ForegroundColor Gray
        $process = Start-Process -FilePath $uninstallTool -ArgumentList "remove", "--sdk", "9.0.301", "--force", "--yes" -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -eq 0) {
            Write-Host "✓ SDK removal completed successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ SDK removal failed with exit code: $($process.ExitCode)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "✗ SDK removal error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "Uninstall tool not found at: $uninstallTool" -ForegroundColor Red
}

Write-Host ""

# Step 2: Remove via Programs and Features (WMI)
Write-Host "Step 2: Removing .NET 9 components via Windows Programs..." -ForegroundColor Yellow

# Get all .NET 9 related products
$dotnet9Products = Get-WmiObject -Class Win32_Product | Where-Object { 
    $_.Name -match "\.NET.*9\.|ASP\.NET Core 9\.|\.NET 9\." -or
    $_.Name -like "*NET*9.0*" -or
    $_.Name -like "*NET 9*"
}

if ($dotnet9Products.Count -gt 0) {
    Write-Host "Found $($dotnet9Products.Count) .NET 9 components:" -ForegroundColor Cyan
    foreach ($product in $dotnet9Products) {
        Write-Host "  - $($product.Name) ($($product.Version))" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Removing components..." -ForegroundColor Yellow
    
    foreach ($product in $dotnet9Products) {
        try {
            Write-Host "Removing: $($product.Name)..." -ForegroundColor Green -NoNewline
            $result = $product.Uninstall()
            if ($result.ReturnValue -eq 0) {
                Write-Host " ✓" -ForegroundColor Green
            } else {
                Write-Host " ✗ (Code: $($result.ReturnValue))" -ForegroundColor Red
            }
        }
        catch {
            Write-Host " ✗ Error: $_" -ForegroundColor Red
        }
    }
} else {
    Write-Host "No .NET 9 components found in Programs list" -ForegroundColor Green
}

Write-Host ""

# Step 3: Manual directory cleanup
Write-Host "Step 3: Cleaning up remaining directories..." -ForegroundColor Yellow

$dotnetPath = "C:\Program Files\dotnet"
$cleanupPaths = @(
    "$dotnetPath\sdk\9.*",
    "$dotnetPath\shared\Microsoft.NETCore.App\9.*",
    "$dotnetPath\shared\Microsoft.AspNetCore.App\9.*",
    "$dotnetPath\shared\Microsoft.WindowsDesktop.App\9.*"
)

foreach ($pattern in $cleanupPaths) {
    $basePath = Split-Path $pattern
    $namePattern = Split-Path $pattern -Leaf
    if (Test-Path $basePath) {
        $paths = Get-ChildItem -Path $basePath -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like $namePattern }
        foreach ($path in $paths) {
            try {
                Write-Host "Removing directory: $($path.FullName)..." -ForegroundColor Green -NoNewline
                Remove-Item $path.FullName -Recurse -Force
                Write-Host " ✓" -ForegroundColor Green
            }
            catch {
                Write-Host " ✗ $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""

# Step 4: Verification
Write-Host "Step 4: Verification..." -ForegroundColor Yellow
Write-Host ""

Write-Host "Remaining .NET SDKs:" -ForegroundColor Cyan
try {
    $sdks = & dotnet --list-sdks 2>$null
    if ($sdks) {
        $sdks | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
        $net9Sdks = $sdks | Where-Object { $_ -like "9.*" }
        if ($net9Sdks) {
            Write-Host "⚠️  WARNING: .NET 9 SDKs still found!" -ForegroundColor Red
        } else {
            Write-Host "✓ No .NET 9 SDKs found" -ForegroundColor Green
        }
    } else {
        Write-Host "  No SDKs found or dotnet command failed" -ForegroundColor Gray
    }
} catch {
    Write-Host "  Error checking SDKs: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Remaining .NET Runtimes:" -ForegroundColor Cyan
try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes) {
        $runtimes | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
        $net9Runtimes = $runtimes | Where-Object { $_ -like "*9.*" }
        if ($net9Runtimes) {
            Write-Host "⚠️  WARNING: .NET 9 Runtimes still found!" -ForegroundColor Red
        } else {
            Write-Host "✓ No .NET 9 Runtimes found" -ForegroundColor Green
        }
    } else {
        Write-Host "  No runtimes found or dotnet command failed" -ForegroundColor Gray
    }
} catch {
    Write-Host "  Error checking runtimes: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Removal Process Complete ===" -ForegroundColor Cyan

$remainingNet9 = Get-WmiObject -Class Win32_Product | Where-Object { 
    $_.Name -match "\.NET.*9\.|ASP\.NET Core 9\.|\.NET 9\." -or
    $_.Name -like "*NET*9.0*" -or
    $_.Name -like "*NET 9*"
}

if ($remainingNet9.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️  Some .NET 9 components may still remain:" -ForegroundColor Red
    foreach ($item in $remainingNet9) {
        Write-Host "  - $($item.Name)" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "You may need to:" -ForegroundColor Yellow
    Write-Host "1. Restart your computer and run this script again" -ForegroundColor Gray
    Write-Host "2. Check Visual Studio Installer for .NET 9 workloads" -ForegroundColor Gray
    Write-Host "3. Manually remove from Control Panel > Programs and Features" -ForegroundColor Gray
} else {
    Write-Host "✅ All .NET 9 components appear to be removed successfully!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Cyan
try {
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
} catch {
    Read-Host "Press Enter to exit"
}
