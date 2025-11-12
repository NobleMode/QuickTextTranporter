@echo off
echo ========================================
echo QuickTextTransporter - Build Loose Bundled
echo ========================================
echo.
echo Building bundled version with loose files...
echo (Includes .NET Runtime - no single file packing)
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishDir=bin\Release\net8.0-windows\win-x64-loose-bundled\publish\

echo.
echo Copying firewall batch files...
echo.
copy /Y setup-firewall.bat bin\Release\net8.0-windows\win-x64-loose-bundled\publish\
copy /Y remove-firewall.bat bin\Release\net8.0-windows\win-x64-loose-bundled\publish\

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Your application is located at:
echo bin\Release\net8.0-windows\win-x64-loose-bundled\publish\
echo.
echo Firewall management files have been copied to the publish folder.
echo.
echo This version includes .NET Runtime but as loose files (not a single .exe).
echo Copy the entire folder to any Windows device!
echo.
pause
