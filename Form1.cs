using System.Diagnostics;
using System.Net;
using System.IO;

namespace QuickTextTranporter
{
    public partial class Form1 : Form
    {
        private NetworkService _networkService = null!;
        private System.Windows.Forms.Timer _textUpdateTimer = null!;
        private System.Windows.Forms.Timer _connectionCheckTimer = null!;
        private System.Windows.Forms.Timer _pingTimer = null!;
        private List<FileInfo> _yourFiles = new();
        private List<FileInfo> _connectedFiles = new();
        private string? _connectedDeviceIP;
        private bool _isUpdatingText = false;
        private CancellationTokenSource? _downloadCancellationTokenSource;
        private bool _allowConnectedModeChange = true;
        private WebServer? _webServer;
        private System.Windows.Forms.Timer _webServerUpdateTimer = null!;

        public Form1()
        {
            InitializeComponent();
            InitializeNetworkService();
            InitializeTimers(); // Moved here
            InitializeListViews(); // Moved here


            SetupEventHandlers();
            CheckFirewallRules();

            // Initial Web Server State
            ddWebServer.Text = "Web Server - Stopped";
            ddWebServer.ForeColor = Color.Red;
        }

        private void InitializeNetworkService()
        {
            _networkService = new NetworkService();
            _networkService.DeviceDiscovered += OnDeviceDiscovered;
            _networkService.TextReceived += OnTextReceived;
            _networkService.FileListReceived += OnFileListReceived;
            _networkService.FileRequested += OnFileRequested;
            _networkService.ConnectionLost += OnConnectionLost;
            _networkService.IncomingConnectionAccepted += OnIncomingConnectionAccepted;

            // Start listening for incoming connections
            _ = _networkService.StartListeningAsync();
        }

        private void InitializeTimers()
        {
            // Text update timer - 500ms debounce
            _textUpdateTimer = new System.Windows.Forms.Timer();
            _textUpdateTimer.Interval = 500;
            _textUpdateTimer.Tick += TextUpdateTimer_Tick;

            // Connection check timer - 1 minute
            _connectionCheckTimer = new System.Windows.Forms.Timer();
            _connectionCheckTimer.Interval = 60000; // 1 minute
            _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
            _connectionCheckTimer.Start();

            // Ping timer - 5 seconds (2-way ping check)
            _pingTimer = new System.Windows.Forms.Timer();
            _pingTimer.Interval = 5000; // 5 seconds
            _pingTimer.Tick += PingTimer_Tick; // Added missing event handler


            // Web Server UI Update Timer
            _webServerUpdateTimer = new System.Windows.Forms.Timer();
            _webServerUpdateTimer.Interval = 1000;
            _webServerUpdateTimer.Tick += WebServerUpdateTimer_Tick;
            _webServerUpdateTimer.Start();
        }

        private void InitializeListViews()
        {
            // Setup Connected Files ListView
            lvConnectedFiles.View = View.Details;
            lvConnectedFiles.FullRowSelect = true;
            lvConnectedFiles.Columns.Add("File Name", 200);
            lvConnectedFiles.Columns.Add("Path", 150);

            // Setup Your Files ListView
            lvYourFiles.View = View.Details;
            lvYourFiles.FullRowSelect = true;
            lvYourFiles.Columns.Add("File Name", 200); // Added missing column
            lvYourFiles.Columns.Add("Path", 150);

            // Setup Web Server ListView
            lvWebServer.View = View.Details;
            lvWebServer.FullRowSelect = true;
            lvWebServer.Columns.Add("File Name", 200);
            lvWebServer.Columns.Add("Size", 100);
        }

        private void SetupEventHandlers()
        {
            // Button events
            btnRefreshCbDevice.Click += BtnRefreshCbDevice_Click;
            btnQuickClear.Click += BtnQuickClear_Click;
            btnYourAddFiles.Click += BtnYourAddFiles_Click;
            btnClearYourFiles.Click += BtnClearYourFiles_Click;

            // ComboBox event
            cbDevice.SelectedIndexChanged += CbDevice_SelectedIndexChanged;

            // CheckBox event
            cbConnectedMode.CheckedChanged += CbConnectedMode_CheckedChanged;

            // TextBox event
            tbYourText.TextChanged += TbYourText_TextChanged;

            // ListView events
            lvYourFiles.DoubleClick += LvYourFiles_DoubleClick;
            lvConnectedFiles.DoubleClick += LvConnectedFiles_DoubleClick;

            // Context menu item for firewall rules
            removeFileToolStripMenuItem.Click += RemoveFirewallRulesToolStripMenuItem_Click;
            enableFirewallRulesToolStripMenuItem.Click += EnableFirewallRulesToolStripMenuItem_Click; // Added missing event handler

            // Web Server events
            toolStripMenuItem1.Click += (s, e) => ToggleWebServer(true);
            toolStripMenuItem2.Click += (s, e) => ToggleWebServer(false);
            openWebToolStripMenuItem.Click += OpenWebToolStripMenuItem_Click;
            copToolStripMenuItem.Click += CopToolStripMenuItem_Click;
            btnRefreshWS.Click += BtnRefreshWS_Click;
            cbWebDevice.SelectedIndexChanged += CbWebDevice_SelectedIndexChanged;

            // Form load event
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            await RefreshDeviceList();
            UpdateConnectedModeUI();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _textUpdateTimer?.Stop();
            _connectionCheckTimer?.Stop();
            _pingTimer?.Stop();
            _downloadCancellationTokenSource?.Cancel();
            _webServerUpdateTimer?.Stop();
            _networkService?.Dispose();
            _webServer?.Stop();
        }

        private void CbConnectedMode_CheckedChanged(object? sender, EventArgs e)
        {
            // Allow programmatic changes (when connection is lost)
            if (!_allowConnectedModeChange)
            {
                return;
            }

            // Prevent manual unchecking when connected
            if (!cbConnectedMode.Checked && _networkService?.IsConnected == true)
            {
                cbConnectedMode.Checked = true;
                MessageBox.Show("Cannot disable Connected Mode while connected to a device.\nDisconnect first or wait for connection to be lost.",
                    "Connected Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UpdateConnectedModeUI();
        }

        private void UpdateConnectedModeUI()
        {
            bool isConnectedMode = cbConnectedMode.Checked;

            // Disable/enable discovery controls based on mode
            cbDevice.Enabled = !isConnectedMode;
            btnRefreshCbDevice.Enabled = !isConnectedMode;

            // Update status bar
            if (isConnectedMode)
            {
                tsConnectedMode.Text = "Connected Mode - ON";
                tsConnectedMode.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                tsConnectedMode.Text = "Connected Mode - OFF";
                tsConnectedMode.ForeColor = System.Drawing.SystemColors.ControlText;
            }
        }

        private async void BtnRefreshCbDevice_Click(object? sender, EventArgs e)
        {
            // Check if Ctrl is held - allows manual IP entry
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                await ConnectManualIP();
            }
            else
            {
                await RefreshDeviceList();
            }
        }

        private async Task ConnectManualIP()
        {
            var ipDialog = new Form
            {
                Text = "Connect to IP Address",
                Width = 350,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Enter IP Address:",
                Left = 10,
                Top = 20,
                AutoSize = true
            };

            var textBox = new TextBox
            {
                Left = 10,
                Top = 45,
                Width = 310
            };

            var okButton = new Button
            {
                Text = "Connect",
                Left = 160,
                Top = 75,
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Left = 245,
                Top = 75,
                DialogResult = DialogResult.Cancel
            };

            ipDialog.Controls.Add(label);
            ipDialog.Controls.Add(textBox);
            ipDialog.Controls.Add(okButton);
            ipDialog.Controls.Add(cancelButton);
            ipDialog.AcceptButton = okButton;
            ipDialog.CancelButton = cancelButton;

            if (ipDialog.ShowDialog() == DialogResult.OK)
            {
                var ipAddress = textBox.Text.Trim();

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // Clear current data
                    tbConnectedText.Clear();
                    lvConnectedFiles.Items.Clear();
                    _connectedFiles.Clear();

                    tsTextStatus.Text = $"Connecting to {ipAddress}...";

                    bool connected = await _networkService.ConnectToDeviceAsync(ipAddress);

                    if (connected)
                    {
                        _connectedDeviceIP = ipAddress;
                        tsTextStatus.Text = $"Connected to {ipAddress}";

                        // Add to combo box
                        var device = new DiscoveredDevice
                        {
                            DeviceName = ipAddress,
                            IPAddress = ipAddress
                        };

                        cbDevice.Items.Add(device);
                        cbDevice.SelectedItem = device;

                        // Send our current text and file list
                        await SendYourTextToDevice();
                        await SendYourFileListToDevice();
                    }
                    else
                    {
                        tsTextStatus.Text = "Connection failed";
                        MessageBox.Show($"Failed to connect to {ipAddress}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async Task RefreshDeviceList()
        {
            cbDevice.Items.Clear();
            cbDevice.Text = "Discovering...";
            btnRefreshCbDevice.Enabled = false;

            try
            {
                var devices = await _networkService.DiscoverDevicesAsync(3000);

                cbDevice.Items.Clear();
                foreach (var device in devices)
                {
                    cbDevice.Items.Add(device);
                }

                if (cbDevice.Items.Count > 0)
                {
                    cbDevice.Text = "Select a device...";
                }
                else
                {
                    cbDevice.Text = "No devices found";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error discovering devices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefreshCbDevice.Enabled = true;
            }
        }

        private async void CbDevice_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cbDevice.SelectedItem is DiscoveredDevice device)
            {
                // Clear current data
                tbConnectedText.Clear();
                lvConnectedFiles.Items.Clear();
                _connectedFiles.Clear();

                // Connect to the device
                tsTextStatus.Text = $"Connecting to {device.DeviceName}...";

                bool connected = await _networkService.ConnectToDeviceAsync(device.IPAddress);

                if (connected)
                {
                    _connectedDeviceIP = device.IPAddress;
                    tsTextStatus.Text = $"Connected to {device.DeviceName}";

                    // DON'T enable Connected Mode - we are the connector, not the receiver
                    // Connected Mode only enables on the device being connected TO

                    // Start ping timer
                    _pingTimer.Start();

                    // Send our current text and file list
                    await SendYourTextToDevice();
                    await SendYourFileListToDevice();
                }
                else
                {
                    tsTextStatus.Text = "Connection failed";
                    MessageBox.Show($"Failed to connect to {device.DeviceName}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TbYourText_TextChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingText) return;

            _textUpdateTimer.Stop(); // Stop to reset debounce
            _textUpdateTimer.Start();

            // Update Web Server Host Text
            if (_webServer != null && _webServer.IsRunning)
            {
                _webServer.HostText = tbYourText.Text;
            }
        }

        private async void TextUpdateTimer_Tick(object? sender, EventArgs e)
        {
            _textUpdateTimer.Stop();
            await SendYourTextToDevice();
        }

        private async Task SendYourTextToDevice()
        {
            if (_networkService.IsConnected)
            {
                await _networkService.SendTextAsync(tbYourText.Text);
            }
        }

        private void BtnQuickClear_Click(object? sender, EventArgs e)
        {
            tbYourText.Clear();
        }

        private async void BtnYourAddFiles_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Select files to share";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var fileInfo = new System.IO.FileInfo(filePath);

                    // Check if file already exists
                    if (!_yourFiles.Any(f => f.FilePath == filePath))
                    {
                        var file = new FileInfo
                        {
                            FileName = fileInfo.Name,
                            FilePath = filePath,
                            FileSize = fileInfo.Length
                        };

                        _yourFiles.Add(file);

                        // Add to ListView with format: File (dir)
                        var item = new ListViewItem(file.FileName);
                        item.SubItems.Add($"({fileInfo.DirectoryName})");
                        item.Tag = file;
                        lvYourFiles.Items.Add(item);
                    }
                }

                // Send updated file list to connected device
                await SendYourFileListToDevice();
            }
        }

        private async Task SendYourFileListToDevice()
        {
            if (_networkService.IsConnected)
            {
                await _networkService.SendFileListAsync(_yourFiles);
            }

            // Update Web Server Files
            if (_webServer != null && _webServer.IsRunning)
            {
                _webServer.HostFiles = _yourFiles;
            }
        }

        private async void BtnClearYourFiles_Click(object? sender, EventArgs e)
        {
            _yourFiles.Clear();
            lvYourFiles.Items.Clear();
            await SendYourFileListToDevice();
        }

        private async void LvYourFiles_DoubleClick(object? sender, EventArgs e)
        {
            if (lvYourFiles.SelectedItems.Count > 0)
            {
                var selectedItem = lvYourFiles.SelectedItems[0];
                if (selectedItem.Tag is FileInfo file)
                {
                    _yourFiles.Remove(file);
                    lvYourFiles.Items.Remove(selectedItem);
                    await SendYourFileListToDevice();
                }
            }
        }

        private async void LvConnectedFiles_DoubleClick(object? sender, EventArgs e)
        {
            if (lvConnectedFiles.SelectedItems.Count > 0)
            {
                var selectedItem = lvConnectedFiles.SelectedItems[0];
                if (selectedItem.Tag is FileInfo file)
                {
                    using var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = file.FileName;
                    saveFileDialog.Title = "Save file as...";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Cancel any previous download
                        _downloadCancellationTokenSource?.Cancel();
                        _downloadCancellationTokenSource = new CancellationTokenSource();

                        // Show progress bar
                        pbTransfer.Visible = true;
                        pbTransfer.Value = 0;

                        try
                        {
                            // Request the file from connected device
                            await _networkService.RequestFileAsync(file.FileName ?? "", saveFileDialog.FileName,
                                new Progress<int>(percent =>
                                {
                                    if (InvokeRequired)
                                    {
                                        Invoke(new Action(() => pbTransfer.Value = percent));
                                    }
                                    else
                                    {
                                        pbTransfer.Value = percent;
                                    }
                                }),
                                _downloadCancellationTokenSource.Token);

                            MessageBox.Show($"File '{file.FileName}' downloaded successfully!",
                                "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (OperationCanceledException)
                        {
                            MessageBox.Show("Download cancelled.",
                                "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Download failed: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            // Hide progress bar
                            pbTransfer.Visible = false;
                            pbTransfer.Value = 0;
                        }
                    }
                }
            }
        }

        private void OnDeviceDiscovered(object? sender, DeviceDiscoveredEventArgs e)
        {
            // This might be called from a background thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnDeviceDiscovered(sender, e)));
                return;
            }

            // Add device to combo box if not already there
            bool exists = false;
            foreach (var item in cbDevice.Items)
            {
                if (item is DiscoveredDevice device && device.IPAddress == e.IPAddress)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                cbDevice.Items.Add(new DiscoveredDevice
                {
                    DeviceName = e.DeviceName,
                    IPAddress = e.IPAddress
                });
            }
        }

        private void OnTextReceived(object? sender, TextReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnTextReceived(sender, e)));
                return;
            }

            _isUpdatingText = true;
            tbConnectedText.Text = e.Text;
            _isUpdatingText = false;
        }

        private void OnFileListReceived(object? sender, FileListReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnFileListReceived(sender, e)));
                return;
            }

            _connectedFiles = e.Files;
            lvConnectedFiles.Items.Clear();

            foreach (var file in _connectedFiles)
            {
                // Format: File (rd - dir)
                var item = new ListViewItem(file.FileName);

                // Extract directory from file path
                string directory = "";
                if (!string.IsNullOrEmpty(file.FilePath))
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(file.FilePath);
                        directory = fileInfo.DirectoryName ?? "";
                    }
                    catch
                    {
                        directory = file.FilePath ?? "";
                    }
                }

                item.SubItems.Add($"(rd - {directory})");
                item.Tag = file;
                lvConnectedFiles.Items.Add(item);
            }
        }

        private async void OnFileRequested(object? sender, FileRequestEventArgs e)
        {
            // Find the requested file in our list
            var file = _yourFiles.FirstOrDefault(f => f.FileName == e.FileName);

            if (file != null && !string.IsNullOrEmpty(file.FilePath) && File.Exists(file.FilePath))
            {
                try
                {
                    await _networkService.SendFileAsync(file.FilePath, file.FileName ?? "");
                }
                catch (Exception ex)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => MessageBox.Show($"Error sending file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                    else
                    {
                        MessageBox.Show($"Error sending file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnConnectionLost(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionLost(sender, e)));
                return;
            }

            tsTextStatus.Text = "Connection lost";
            _connectedDeviceIP = null;

            // Stop ping timer
            _pingTimer.Stop();

            // Auto-disable Connected Mode - bypass the check since connection is lost
            _allowConnectedModeChange = false;
            cbConnectedMode.Checked = false;
            _allowConnectedModeChange = true;
            UpdateConnectedModeUI();

            // Cancel any ongoing downloads
            _downloadCancellationTokenSource?.Cancel();
        }

        private async void PingTimer_Tick(object? sender, EventArgs e)
        {
            if (_networkService.IsConnected)
            {
                // Send ping to keep connection alive
                await _networkService.SendPingAsync();
            }
            else
            {
                // Connection lost
                _pingTimer.Stop();
                OnConnectionLost(this, EventArgs.Empty);
            }
        }

        private void OnIncomingConnectionAccepted(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnIncomingConnectionAccepted(sender, e)));
                return;
            }

            // Someone connected TO this device - enable Connected Mode
            _allowConnectedModeChange = false;
            cbConnectedMode.Checked = true;
            _allowConnectedModeChange = true;
            UpdateConnectedModeUI();

            // Start ping timer
            _pingTimer.Start();

            tsTextStatus.Text = "Device connected to you";
        }

        private void ConnectionCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (_networkService.IsConnected)
            {
                // Connection is active
                if (cbDevice.SelectedItem is DiscoveredDevice device)
                {
                    tsTextStatus.Text = $"Connected to {device.DeviceName}";
                }
                else
                {
                    tsTextStatus.Text = "Connected";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_connectedDeviceIP))
                {
                    tsTextStatus.Text = "Connection lost";
                    _connectedDeviceIP = null;
                }
                else
                {
                    tsTextStatus.Text = "No connected device";
                }
            }
        }

        private void EnableFirewallRulesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                // Check if running as administrator
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

                if (!isAdmin)
                {
                    MessageBox.Show(
                        "Administrator privileges are required to add firewall rules.\n\n" +
                        "Please restart the application as Administrator and try again.",
                        "Administrator Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Add UDP rule for device discovery (port 45678)
                var udpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall add rule name=\"QuickTextTransporter UDP\" dir=in action=allow protocol=UDP localport=45678",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                udpProcess.Start();
                udpProcess.WaitForExit();

                // Add TCP rule for communication (port 45679)
                var tcpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall add rule name=\"QuickTextTransporter TCP\" dir=in action=allow protocol=TCP localport=45679",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                tcpProcess.Start();
                tcpProcess.WaitForExit();

                if (udpProcess.ExitCode == 0 && tcpProcess.ExitCode == 0)
                {
                    MessageBox.Show(
                        "✅ Firewall Rules Added Successfully!\n\n" +
                        "The following rules have been configured:\n" +
                        "• UDP Port 45678 (Device Discovery)\n" +
                        "• TCP Port 45679 (Communication)\n\n" +
                        "QuickTextTransporter can now communicate on your network.",
                        "Firewall Configuration Changed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "❌ Failed to Add Firewall Rules\n\n" +
                        "Possible reasons:\n" +
                        "• Rules may already exist\n" +
                        "• Insufficient permissions\n" +
                        "• Firewall service unavailable\n\n" +
                        "Please check Windows Firewall settings manually.",
                        "Firewall Configuration Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error Adding Firewall Rules\n\n" +
                    $"Error Details:\n{ex.Message}",
                    "Firewall Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                CheckFirewallRules();
            }
        }

        private void RemoveFirewallRulesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                // Check if running as administrator
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

                if (!isAdmin)
                {
                    MessageBox.Show(
                        "Administrator privileges are required to remove firewall rules.\n\n" +
                        "Please restart the application as Administrator and try again.",
                        "Administrator Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "⚠️ Remove Firewall Rules?\n\n" +
                    "This will remove the following rules:\n" +
                    "• QuickTextTransporter UDP (Port 45678)\n" +
                    "• QuickTextTransporter TCP (Port 45679)\n\n" +
                    "The application may not work properly without these rules.\n\n" +
                    "Do you want to continue?",
                    "Confirm Firewall Configuration Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                // Remove UDP rule
                var udpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall delete rule name=\"QuickTextTransporter UDP\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                udpProcess.Start();
                udpProcess.WaitForExit();

                // Remove TCP rule
                var tcpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "advfirewall firewall delete rule name=\"QuickTextTransporter TCP\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                tcpProcess.Start();
                tcpProcess.WaitForExit();

                MessageBox.Show(
                    "✅ Firewall Rules Removed Successfully!\n\n" +
                    "The following rules have been deleted:\n" +
                    "• QuickTextTransporter UDP (Port 45678)\n" +
                    "• QuickTextTransporter TCP (Port 45679)\n\n" +
                    "Note: You may need to add them back to use the application.",
                    "Firewall Configuration Changed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error Removing Firewall Rules\n\n" +
                    $"Error Details:\n{ex.Message}",
                    "Firewall Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                CheckFirewallRules();
            }
        }

        private void CheckFirewallRules()
        {
            Task.Run(() =>
            {
                try
                {
                    // Check UDP Rule
                    var udpProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "advfirewall firewall show rule name=\"QuickTextTransporter UDP\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true
                        }
                    };
                    udpProcess.Start();
                    string udpOutput = udpProcess.StandardOutput.ReadToEnd();
                    udpProcess.WaitForExit();

                    // Check TCP Rule
                    var tcpProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "advfirewall firewall show rule name=\"QuickTextTransporter TCP\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true
                        }
                    };
                    tcpProcess.Start();
                    string tcpOutput = tcpProcess.StandardOutput.ReadToEnd();
                    tcpProcess.WaitForExit();

                    bool rulesExist = !udpOutput.Contains("No rules match") && !tcpOutput.Contains("No rules match");

                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => UpdateFirewallStatus(rulesExist)));
                    }
                    else
                    {
                        UpdateFirewallStatus(rulesExist);
                    }
                }
                catch
                {
                    // Ignore errors during check
                }
            });
        }

        private void UpdateFirewallStatus(bool enabled)
        {
            if (enabled)
            {
                ddFirewall.Text = "Firewall - Rules Active";
                ddFirewall.ForeColor = Color.Green;
                enableFirewallRulesToolStripMenuItem.Enabled = false;
                removeFileToolStripMenuItem.Enabled = true;
            }
            else
            {
                ddFirewall.Text = "Firewall - Rules Missing";
                ddFirewall.ForeColor = Color.Red;
                enableFirewallRulesToolStripMenuItem.Enabled = true;
                removeFileToolStripMenuItem.Enabled = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _webServer?.Stop();
            base.OnFormClosing(e);
        }

        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost";
        }

        private void ToggleWebServer(bool enable)
        {
            if (enable)
            {
                if (_webServer == null)
                {
                    _webServer = new WebServer();
                    _webServer.HostText = tbYourText.Text;
                    _webServer.HostFiles = _yourFiles;
                    _webServer.ClientConnected += (s, ip) => Invoke(new Action(() => RefreshWebClients()));
                    _webServer.TextReceived += (s, data) => Invoke(new Action(() =>
                    {
                        if (cbWebDevice.SelectedItem?.ToString() == data.Ip)
                        {
                            tbWebServer.Text = data.Text;
                        }
                    }));
                    // Add FileReceived handler to refresh list when file is uploaded
                    _webServer.ClientUpdated += (s, ip) => Invoke(new Action(() =>
                    {
                        if (cbWebDevice.SelectedItem?.ToString() == ip)
                        {
                            // Trigger a refresh of the file list for the selected client
                            // We can reuse the selection change logic or just force a refresh
                            CbWebDevice_SelectedIndexChanged(null, EventArgs.Empty);
                        }
                    }));
                    _webServer.ClientDisconnected += (s, ip) => Invoke(new Action(() =>
                    {
                        RefreshWebClients();
                        // If the disconnected client was selected, clear the selection or select the next one
                        // RefreshWebClients handles repopulating the list, so we just need to ensure the UI state is consistent.
                        // If the list becomes empty, RefreshWebClients handles that too.
                    }));
                }

                try
                {
                    _webServer.Start();
                    string ip = GetLocalIpAddress();
                    ddWebServer.Text = $"http://{ip}:45680";
                    ddWebServer.ForeColor = Color.Green;
                    toolStripMenuItem1.Enabled = false;
                    toolStripMenuItem2.Enabled = true;

                    // Force refresh to show empty state if needed
                    RefreshWebClients();
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 5) // Access Denied
                {
                    var result = MessageBox.Show(
                        "Web Server requires Administrator privileges to listen on port 45680.\n\n" +
                        "Do you want to restart the application as Administrator?",
                        "Administrator Required",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        var processInfo = new ProcessStartInfo(Application.ExecutablePath)
                        {
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        try
                        {
                            Process.Start(processInfo);
                            Application.Exit();
                        }
                        catch
                        {
                            // User cancelled UAC
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start Web Server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _webServer?.Stop();
                ddWebServer.Text = "Web Server - Stopped";
                ddWebServer.ForeColor = Color.Red;
                toolStripMenuItem1.Enabled = true;
                toolStripMenuItem2.Enabled = false;
                tsWebServerCount.Text = "Web Server - 0 Devices";
                cbWebDevice.Items.Clear();
                cbWebDevice.Items.Add("No devices connected");
                cbWebDevice.SelectedIndex = 0;
                tbWebServer.Clear();
                lvWebServer.Items.Clear();
            }
        }

        private void BtnRefreshWS_Click(object? sender, EventArgs e)
        {
            RefreshWebClients();
        }

        private void RefreshWebClients()
        {
            if (_webServer == null || !_webServer.IsRunning) return;

            var currentSelection = cbWebDevice.SelectedItem?.ToString();
            cbWebDevice.Items.Clear();

            foreach (var client in _webServer.ActiveClients)
            {
                cbWebDevice.Items.Add(client);
            }

            if (cbWebDevice.Items.Count == 0)
            {
                cbWebDevice.Items.Add("No devices connected");
                cbWebDevice.SelectedIndex = 0;
            }
            else if (currentSelection != null && cbWebDevice.Items.Contains(currentSelection))
            {
                cbWebDevice.SelectedItem = currentSelection;
            }
            else
            {
                cbWebDevice.SelectedIndex = 0;
            }

            tsWebServerCount.Text = $"Web Server - {_webServer.ClientCount} Devices";
        }

        private void CbWebDevice_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Clear current view
            tbWebServer.Clear();
            lvWebServer.Items.Clear();

            if (_webServer != null && cbWebDevice.SelectedItem is string ip)
            {
                var client = _webServer.GetClientState(ip);
                if (client != null)
                {
                    tbWebServer.Text = client.Text;

                    // Populate uploaded files
                    foreach (var file in client.UploadedFiles)
                    {
                        var item = new ListViewItem(file.Name);
                        item.SubItems.Add(FormatFileSize(file.Length));
                        item.Tag = file.FullName; // Store full path for potential opening/interaction
                        lvWebServer.Items.Add(item);
                    }
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void WebServerUpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_webServer != null && _webServer.IsRunning)
            {
                tsWebServerCount.Text = $"Web Server - {_webServer.ClientCount} Devices";

                // Also update text if the selected client updated
                if (cbWebDevice.SelectedItem is string ip)
                {
                    var client = _webServer.GetClientState(ip);
                    if (client != null)
                    {
                        if (tbWebServer.Text != client.Text && !tbWebServer.Focused)
                        {
                            tbWebServer.Text = client.Text;
                        }

                        // Check if file list needs update (simple count check for now)
                        if (lvWebServer.Items.Count != client.UploadedFiles.Count)
                        {
                            // Refresh list
                            lvWebServer.Items.Clear();
                            foreach (var file in client.UploadedFiles)
                            {
                                var item = new ListViewItem(file.Name);
                                item.SubItems.Add(FormatFileSize(file.Length));
                                item.Tag = file.FullName;
                                lvWebServer.Items.Add(item);
                            }
                        }
                    }
                }
            }
        }
        private void OpenWebToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            EnsureWebServerRunning(() =>
            {
                string ip = GetLocalIpAddress();
                string url = $"http://{ip}:45680";
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void CopToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            EnsureWebServerRunning(() =>
            {
                string ip = GetLocalIpAddress();
                string url = $"http://{ip}:45680";
                Clipboard.SetText(url);
                MessageBox.Show("Web Server link copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void EnsureWebServerRunning(Action action)
        {
            if (_webServer != null && _webServer.IsRunning)
            {
                action();
            }
            else
            {
                var result = MessageBox.Show("Web Server is not running. Do you want to start it?", "Web Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ToggleWebServer(true);
                    if (_webServer != null && _webServer.IsRunning)
                    {
                        action();
                    }
                }
            }
        }
    }
}
