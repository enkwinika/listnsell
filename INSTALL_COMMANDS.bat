@echo off
echo ====================================================
echo Installing Required NuGet Packages
echo ====================================================
echo.
echo IMPORTANT: Run this in Visual Studio Package Manager Console
echo Tools -^> NuGet Package Manager -^> Package Manager Console
echo.
echo Then copy and paste these commands one by one:
echo.
echo ====================================================
echo.
echo # Install Dependency Injection packages
echo Install-Package Ninject -Version 3.3.4
echo Install-Package Ninject.Web.Common -Version 3.3.2
echo Install-Package Ninject.Web.Common.WebHost -Version 3.3.2
echo Install-Package Ninject.MVC5 -Version 3.3.2
echo Install-Package WebActivatorEx -Version 2.2.0
echo.
echo # Install Logging packages
echo Install-Package NLog -Version 4.7.15
echo Install-Package NLog.Web -Version 4.14.0
echo Install-Package NLog.Config -Version 4.7.15
echo.
echo ====================================================
pause
