@echo off
echo ========================================
echo QuickTextTransporter Firewall Cleanup
echo ========================================
echo.
echo This script will REMOVE firewall rules for QuickTextTransporter
echo.
echo You need to run this script as Administrator!
echo.
pause

echo.
echo Removing UDP rule for device discovery (port 45678)...
netsh advfirewall firewall delete rule name="QuickTextTransporter - Discovery (UDP)"
netsh advfirewall firewall delete rule name="QuickTextTransporter - Discovery (UDP) Out"

echo.
echo Removing TCP rule for communication (port 45679)...
netsh advfirewall firewall delete rule name="QuickTextTransporter - Communication (TCP)"
netsh advfirewall firewall delete rule name="QuickTextTransporter - Communication (TCP) Out"

echo.
echo ========================================
echo Firewall rules removed successfully!
echo ========================================
echo.
pause
