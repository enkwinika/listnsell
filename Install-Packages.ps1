# Install Required NuGet Packages for Refactored rexell Project
# Run this script in Package Manager Console in Visual Studio

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Installing NuGet Packages for rexell Refactoring" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Dependency Injection Packages
Write-Host "Installing Dependency Injection packages..." -ForegroundColor Yellow
Install-Package Ninject -Version 3.3.4
Install-Package Ninject.Web.Common -Version 3.3.2
Install-Package Ninject.Web.Common.WebHost -Version 3.3.2  
Install-Package Ninject.MVC5 -Version 3.3.2
Install-Package WebActivatorEx -Version 2.2.0

Write-Host ""
Write-Host "DI packages installed successfully!" -ForegroundColor Green
Write-Host ""

# Logging Packages
Write-Host "Installing Logging packages..." -ForegroundColor Yellow
Install-Package NLog -Version 4.7.15
Install-Package NLog.Web -Version 4.14.0
Install-Package NLog.Config -Version 4.7.15

Write-Host ""
Write-Host "Logging packages installed successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " All packages installed successfully!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Clean and Rebuild the solution" -ForegroundColor White
Write-Host "2. Verify NLog.config is set to 'Copy if newer'" -ForegroundColor White
Write-Host "3. Run the application and check logs/ folder" -ForegroundColor White
Write-Host ""
