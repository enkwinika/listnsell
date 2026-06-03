# ====================================
# ?? FORCE CLEAN BUILD & FIX INTELLISENSE
# ====================================
# This script clears all caches and forces a clean rebuild

Write-Host "?? FORCE CLEAN BUILD & FIX INTELLISENSE" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean bin and obj folders
Write-Host "?? Step 1: Cleaning bin and obj folders..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Remove-Item -Path "bin" -Recurse -Force
    Write-Host "   ? Deleted bin folder" -ForegroundColor Green
}
if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force
    Write-Host "   ? Deleted obj folder" -ForegroundColor Green
}

# Step 2: Clean .vs folder (Visual Studio cache)
Write-Host ""
Write-Host "?? Step 2: Cleaning .vs folder (VS cache)..." -ForegroundColor Yellow
if (Test-Path ".vs") {
    Remove-Item -Path ".vs" -Recurse -Force
    Write-Host "   ? Deleted .vs folder" -ForegroundColor Green
}

# Step 3: Clean packages folder temporary files
Write-Host ""
Write-Host "?? Step 3: Cleaning packages temp files..." -ForegroundColor Yellow
if (Test-Path "packages\.temp") {
    Remove-Item -Path "packages\.temp" -Recurse -Force
    Write-Host "   ? Cleaned packages temp" -ForegroundColor Green
}

# Step 4: Clear ASP.NET Temp Files
Write-Host ""
Write-Host "?? Step 4: Clearing ASP.NET temp files..." -ForegroundColor Yellow
$aspnetTemp = "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files"
if (Test-Path $aspnetTemp) {
    try {
        Remove-Item -Path $aspnetTemp -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   ? Cleared ASP.NET temp files" -ForegroundColor Green
    } catch {
        Write-Host "   ??  Some files in use, partial clean" -ForegroundColor Yellow
    }
}

# Step 5: Clear RoslynCache
Write-Host ""
Write-Host "?? Step 5: Clearing Roslyn cache..." -ForegroundColor Yellow
$roslynCache = "$env:LOCALAPPDATA\Temp\VBCSCompiler"
if (Test-Path $roslynCache) {
    try {
        Remove-Item -Path $roslynCache -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   ? Cleared Roslyn cache" -ForegroundColor Green
    } catch {
        Write-Host "   ??  Some files in use" -ForegroundColor Yellow
    }
}

# Step 6: Verify Views/Web.config
Write-Host ""
Write-Host "?? Step 6: Verifying Views/Web.config..." -ForegroundColor Yellow
$viewsWebConfig = "Views\Web.config"
if (Test-Path $viewsWebConfig) {
    $content = Get-Content $viewsWebConfig -Raw
    if ($content -match "System.Linq" -and $content -match "System.Core" -and $content -match "Microsoft.CSharp") {
        Write-Host "   ? Views/Web.config is correct" -ForegroundColor Green
    } else {
        Write-Host "   ? Views/Web.config needs update!" -ForegroundColor Red
        Write-Host "   Run this in Package Manager Console:" -ForegroundColor Yellow
        Write-Host "   Update-Package -Reinstall" -ForegroundColor Cyan
    }
} else {
    Write-Host "   ? Views/Web.config not found!" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "? CLEANUP COMPLETE!" -ForegroundColor Green
Write-Host ""
Write-Host "?? NEXT STEPS:" -ForegroundColor Cyan
Write-Host "   1. Close Visual Studio completely" -ForegroundColor White
Write-Host "   2. Reopen the solution" -ForegroundColor White
Write-Host "   3. Rebuild Solution (Ctrl+Shift+B)" -ForegroundColor White
Write-Host "   4. Close and reopen any .cshtml files" -ForegroundColor White
Write-Host ""
Write-Host "? IntelliSense errors should disappear after VS restarts!" -ForegroundColor Green
Write-Host ""
