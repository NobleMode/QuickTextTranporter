# Quick Start Guide - QuickTextTransporter

## Setup (First Time)

### Step 1: Build and Run

**For Development**:
```powershell
cd "f:\Code\Desktop App\QuickTextTranporter"
dotnet run
```

**For Distribution** - Choose one build option:

1. **Small build** (requires .NET 8.0 Runtime):
```powershell
build-small-exe.bat
# Output: bin\Release\net8.0-windows\win-x64\publish\
```

2. **Single EXE build** (no runtime needed):
```powershell
build-exe.bat
# Output: bin\Release\net8.0-windows\win-x64-bundled\publish\
```

3. **Loose files build** (no runtime needed, faster startup):
```powershell
build-loose-bundled.bat
# Output: bin\Release\net8.0-windows\win-x64-loose-bundled\publish\
```

All builds include firewall setup scripts automatically.

### Step 2: Configure Windows Firewall

When you first run the application, Windows may prompt you to allow network access. **Click "Allow access"** for both Private and Public networks.

**Easy Setup** (Recommended):
- Right-click `setup-firewall.bat` and select "Run as administrator"
- This automatically configures both UDP and TCP ports
- To remove later, run `remove-firewall.bat` as administrator

**Manual Setup** (if needed):
1. Press `Win + R`, type `wf.msc`, and press Enter
2. Click "Inbound Rules" → "New Rule"
3. Select "Port" → Next
4. Select "TCP", enter port `45679` → Next
5. Allow the connection → Next
6. Check all network types → Next
7. Name it "QuickTextTransporter TCP" → Finish
8. Repeat for UDP port `45678`

## Running on Two Devices

### Device 1 (e.g., Your Laptop)
1. Run the application
2. It will automatically start listening for other devices

### Device 2 (e.g., Your PC)
1. Run the application
2. Click the **Refresh** button
3. Wait 2-3 seconds for devices to be discovered
4. Select "Device 1" from the dropdown
5. You should see "Connected to Device 1" in the status bar

## Basic Usage

### Sending Text
1. Type in the "Your Device" text box on the right
2. After you stop typing for 500ms, text automatically sends
3. The text appears in "Connected Device" on the other device

### Sending Files
1. Click **Add** button (right side)
2. Select one or more files
3. Files appear in "Your Device Files" list
4. They immediately appear on the connected device's "Connected Device Files" list

### Receiving Files
1. See files in "Connected Device Files" (left side)
2. Double-click a file to download
3. Choose where to save it

### Removing Shared Files
- Double-click a file in "Your Device Files" to unshare it
- Click **Clear** to remove all your shared files

## Testing on One Machine

You can test on a single machine by running two instances:

```powershell
# Terminal 1
cd "f:\Code\Desktop App\QuickTextTranporter"
dotnet run

# Terminal 2 (new window)
cd "f:\Code\Desktop App\QuickTextTranporter"
dotnet run
```

Both instances will discover each other via localhost!

## Troubleshooting

### "No devices found"
- Ensure both devices are on the same network
- Check that both applications are running
- Verify firewall allows UDP 45678 and TCP 45679
- Try running as administrator

### "Connection failed"
- Target device may have closed the application
- Firewall may be blocking TCP connections
- Try refreshing and reconnecting

### Text not syncing
- Check status bar shows "Connected to [device]"
- Wait at least 500ms after typing
- Try reconnecting

### Files not visible
- Ensure file is still selected on sending device
- Check connection status
- Re-add the file if needed

## Network Architecture

```
Device A                                    Device B
┌──────────────────┐                   ┌──────────────────┐
│                  │                   │                  │
│  QuickText App   │                   │  QuickText App   │
│                  │                   │                  │
│  UDP Listener    │◄──────────────────┤  UDP Broadcast   │
│  (Port 45678)    │                   │  (Discovery)     │
│                  │                   │                  │
│  TCP Listener    │◄──────────────────┤  TCP Client      │
│  (Port 45679)    │      Connect      │  (Port 45679)    │
│                  │                   │                  │
│  TCP Client      ├───────────────────►  TCP Listener    │
│  (Port 45679)    │   Send Data       │  (Port 45679)    │
│                  │                   │                  │
└──────────────────┘                   └──────────────────┘
```

## Features Summary

✅ Automatic device discovery on local network
✅ Real-time text synchronization (500ms debounce)
✅ File sharing with simple UI
✅ Connection status monitoring (1-minute intervals)
✅ Support for multiple file types
✅ Simple and lightweight

## Performance Notes

- Discovery timeout: 3 seconds
- Text sync delay: 500ms after typing stops
- Connection check: Every 60 seconds
- No file size limit (but large files use more memory)

## Support

For issues or questions, check:
- README.md for detailed documentation
- Ensure .NET 8.0 Windows runtime is installed
- Verify network connectivity between devices
