@echo off
echo ========================================
echo QuickTextTransporter - Build Small EXE
echo ========================================
echo.
echo Building framework-dependent executable...
echo (Requires .NET 8.0 Runtime to be installed)
echo.

dotnet publish -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Your executable is located at:
echo bin\Release\net8.0-windows\win-x64\publish\QuickTextTranporter.exe
echo.
echo This version is MUCH smaller but requires .NET 8.0 Runtime!
echo.
echo If .NET 8.0 is not installed, download from:
echo https://dotnet.microsoft.com/download/dotnet/8.0
echo.
pause
