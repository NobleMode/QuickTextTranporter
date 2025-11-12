# QuickTextTransporter

A simple Windows Forms application for sharing text and files between devices on a local network.

## Features

- **Device Discovery**: Automatically discover other devices running the app on your local network (multi-interface support for WiFi/LAN mixed networks)
- **Real-time Text Sync**: Text changes are synchronized with a 500ms debounce
- **File Sharing**: Share files between devices with progress tracking
- **Auto Connected Mode**: Receiver automatically enables Connected Mode for seamless two-way sync
- **Connection Monitoring**: 2-way ping/pong system (every 5 seconds) with automatic connection loss detection
- **File Download Progress**: Visual progress bar shows download status with percentage
- **Debug Console**: Optional console window for troubleshooting and monitoring network activity
- **Dual Build Options**: Self-contained (154MB) or framework-dependent (0.2MB) builds available

## How It Works

The application uses:
- **UDP Broadcast** (Port 45678) for device discovery on the local network
- **TCP Connection** (Port 45679) for peer-to-peer communication between devices

## Usage

### Getting Started

1. **Launch the application** on both devices (e.g., your laptop and PC)
2. Both devices must be on the same local network (WiFi, Ethernet, or mixed)

### Discovering Devices

1. Click the **Refresh** button next to the device dropdown
2. The app will scan the network for 3 seconds across all network interfaces
3. Available devices will appear in the dropdown menu showing: `DeviceName (IP:Port)`
4. Select a device to connect
5. **Advanced**: Hold **Ctrl** while clicking Refresh to manually enter an IP address

**Auto Connected Mode**: When you connect to another device, that device automatically enables Connected Mode to sync changes back to you

### Sharing Text

- **Your Device**: Type text in the "Your Device" text box
  - Text is automatically sent to the connected device after 500ms of no typing
  - Use the **Clear** button to quickly clear your text
  
- **Connected Device**: View the connected device's text in the read-only "Connected Device" text box
  - You can select and copy text from this box

### Sharing Files

#### Your Files (Right Side)
- Click **Add** to browse and select files to share
- Files appear in the list with format: `FileName (directory)`
- **Double-click** a file to remove it from the shared list
- Click **Clear** to remove all files at once

#### Connected Device Files (Left Side)
- View files shared by the connected device
- Format: `FileName (rd - directory)` where "rd" means remote device
- **Double-click** a file to download it (prompts for save location)
- Progress bar shows download status with percentage
- Save dialog automatically sets file type filter based on extension

### Connection Status & Connected Mode

The status bar at the bottom shows:
- "No connected device" - Not connected to any device
- "Connected to [DeviceName]" - Successfully connected
- "Connection lost" - Connection was interrupted (Connected Mode automatically disables)

**Connected Mode Checkbox**:
- Automatically enabled when someone connects to your device
- Keeps the connection active and syncs your changes back to the connected device
- Automatically disables when connection is lost to prevent issues
- Manually toggle it to control two-way synchronization

**Ping System**: Connection health is monitored with 2-way ping/pong every 5 seconds, ensuring quick detection of disconnections.

## Network Requirements

- Both devices must be on the same local network subnet
- Firewall must allow:
  - UDP port 45678 (device discovery)
  - TCP port 45679 (communication)

### Windows Firewall Configuration

Use the included batch scripts for easy firewall setup:

**Option 1 - Automated Setup** (Recommended):
- Run `setup-firewall.bat` as Administrator to automatically configure firewall rules
- Run `remove-firewall.bat` as Administrator to remove the rules later

**Option 2 - Manual Setup**:
1. Open **Windows Defender Firewall**
2. Click **Allow an app through firewall**
3. Find **QuickTextTransporter** or click **Allow another app**
4. Ensure both **Private** and **Public** are checked

## Building the Application

Two build options are available:

### Self-Contained Build (154MB)
- Includes .NET 8.0 runtime
- No prerequisites needed on target machine
- Run: `build-exe.bat`
- Output: `bin\Release\net8.0-windows\win-x64\publish\`

### Framework-Dependent Build (0.2MB)
- Requires .NET 8.0 Runtime installed on target machine
- Much smaller file size
- Run: `build-small-exe.bat`
- Output: `bin\Release\net8.0-windows\publish\`

The application automatically checks for .NET 8.0 runtime on startup and shows an error if missing.

## Troubleshooting

**No devices found:**
- Ensure both devices are on the same network
- Check firewall settings (use `setup-firewall.bat`)
- Try running the app as Administrator
- Enable debug console (see below) to check network discovery

**Connection failed:**
- The target device may have firewall restrictions
- Verify the target device is still running the application
- Try refreshing and reconnecting
- Check the debug console for connection errors

**Text not syncing:**
- Check the connection status in the status bar
- Text syncs automatically after 500ms of no typing
- Ensure Connected Mode is enabled on the receiving device
- Connection lost? Connected Mode will auto-disable - reconnect to resume

**Files not downloading:**
- Ensure the connection is stable (check ping status in debug console)
- Large files may take time - watch the progress bar
- Connection interrupted? The download will fail - reconnect and retry

**Connected Mode Issues:**
- Connected Mode automatically enables when someone connects to you
- It automatically disables when connection is lost to prevent errors
- If stuck, reconnect to the device to reset the state

### Debug Console

The application includes a debug console that shows detailed logging:
- Device discovery broadcasts to all network interfaces
- Incoming connection attempts
- Message send/receive details
- Ping/pong heartbeat monitoring
- Connection loss detection
- File transfer progress

To use the debug console:
1. The console window opens automatically alongside the main window
2. Watch for detailed logs about network operations
3. Useful for diagnosing connection issues

## Technical Details

- Built with .NET 8.0 and Windows Forms
- Uses asynchronous network communication (async/await patterns)
- Multi-interface UDP broadcasts for device discovery (port 45678)
- Peer-to-peer TCP connections for data transfer (port 45679)
- Length-prefixed message framing for reliable communication
- JSON serialization for structured messages
- Files are transferred using Base64 encoding
- 500ms debounce timer for text synchronization
- 5-second ping/pong heartbeat for connection monitoring
- Automatic connection loss detection and cleanup
- Event-driven architecture with proper resource disposal

## Limitations

- Only one active connection at a time per device
- File transfer has size limitations based on memory (Base64 encoding increases size by ~33%)
- Discovery limited to local network (no internet/remote connections)
- Both devices must have the application running simultaneously
- Large files may cause delays due to Base64 encoding/decoding

## Testing

For local testing with two instances on the same machine:
- Run `debug-two-instances.bat` to launch two instances in separate windows
- Each instance will have its own device name and can discover the other

## Keyboard Shortcuts

- **Ctrl + Click Refresh**: Manually enter IP address for direct connection (bypasses discovery)

## Future Enhancements

- Multiple simultaneous connections
- Chunked file transfer for large files (streaming)
- File transfer history with retry capability
- True drag-and-drop file support
- System tray integration with notifications
- Encryption for secure communication
- Cross-platform support (Linux, macOS)
