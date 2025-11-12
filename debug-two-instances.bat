@echo off
echo ========================================
echo QuickTextTransporter - Debug Mode
echo ========================================
echo.
echo This will launch TWO instances for testing
echo.
echo Each instance will have its own console window
echo showing debug output.
echo.
pause

echo.
echo Starting Instance 1...
start "Instance 1 - DEBUG" dotnet run

timeout /t 2 > nul

echo Starting Instance 2...
start "Instance 2 - DEBUG" dotnet run

echo.
echo Both instances are running!
echo Check the two windows that just opened.
echo.
pause
