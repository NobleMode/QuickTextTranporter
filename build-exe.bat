@echo off
echo ========================================
echo QuickTextTransporter - Build Executable
echo ========================================
echo.
echo Building standalone executable with .NET included...
echo.

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishDir=bin\Release\net8.0-windows\win-x64-bundled\

echo.
echo Copying firewall batch files...
echo.
copy /Y setup-firewall.bat bin\Release\net8.0-windows\win-x64-bundled\
copy /Y remove-firewall.bat bin\Release\net8.0-windows\win-x64-bundled\
copy /Y CONNECTIVITY_GUIDE.md bin\Release\net8.0-windows\win-x64-bundled\

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Your executable is located at:
echo bin\Release\net8.0-windows\win-x64-bundled\QuickTextTranporter.exe
echo.
echo Firewall management files have been copied to the publish folder.
echo.
echo You can copy the entire folder to any Windows device!
echo.
pause
