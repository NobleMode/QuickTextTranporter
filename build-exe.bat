@echo off
echo ========================================
echo QuickTextTransporter - Build Executable
echo ========================================
echo.
echo Building standalone executable...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Your executable is located at:
echo bin\Release\net8.0-windows\win-x64\publish\QuickTextTranporter.exe
echo.
echo You can copy this single .exe file to any Windows device!
echo.
pause
