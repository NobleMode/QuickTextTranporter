@echo off
echo ========================================
echo QuickTextTransporter Firewall Setup
echo ========================================
echo.
echo This script will add firewall rules for QuickTextTransporter
echo.
echo You need to run this script as Administrator!
echo.
pause

echo.
echo Adding UDP rule for device discovery (port 45678)...
netsh advfirewall firewall add rule name="QuickTextTransporter - Discovery (UDP)" dir=in action=allow protocol=UDP localport=45678
netsh advfirewall firewall add rule name="QuickTextTransporter - Discovery (UDP) Out" dir=out action=allow protocol=UDP localport=45678

echo.
echo Adding TCP rule for communication (port 45679)...
netsh advfirewall firewall add rule name="QuickTextTransporter - Communication (TCP)" dir=in action=allow protocol=TCP localport=45679
netsh advfirewall firewall add rule name="QuickTextTransporter - Communication (TCP) Out" dir=out action=allow protocol=TCP localport=45679

echo.
echo ========================================
echo Firewall rules added successfully!
echo ========================================
echo.
echo You can now run QuickTextTransporter
echo.
pause
