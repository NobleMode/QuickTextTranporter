using System.Diagnostics;

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

        public Form1()
        {
            InitializeComponent();
            InitializeNetworkService();
            InitializeTimers();
            InitializeListViews();
            SetupEventHandlers();
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
            _pingTimer.Tick += PingTimer_Tick;
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
            lvYourFiles.Columns.Add("File Name", 200);
            lvYourFiles.Columns.Add("Path", 150);
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
            _networkService?.Dispose();
        }

        private void CbConnectedMode_CheckedChanged(object? sender, EventArgs e)
        {
            Console.WriteLine($"[DEBUG] CbConnectedMode_CheckedChanged - Checked: {cbConnectedMode.Checked}, AllowChange: {_allowConnectedModeChange}, IsConnected: {_networkService?.IsConnected}");

            // Allow programmatic changes (when connection is lost)
            if (!_allowConnectedModeChange)
            {
                Console.WriteLine($"[DEBUG] Change not allowed, returning");
                return;
            }

            // Prevent manual unchecking when connected
            if (!cbConnectedMode.Checked && _networkService?.IsConnected == true)
            {
                Console.WriteLine($"[DEBUG] Preventing manual uncheck - still connected");
                cbConnectedMode.Checked = true;
                MessageBox.Show("Cannot disable Connected Mode while connected to a device.\nDisconnect first or wait for connection to be lost.", 
                    "Connected Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            Console.WriteLine($"[DEBUG] Updating UI");
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

            // Reset the timer - this creates the 500ms debounce
            _textUpdateTimer.Stop();
            _textUpdateTimer.Start();
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

            Console.WriteLine($"[DEBUG] OnConnectionLost called");

            tsTextStatus.Text = "Connection lost";
            _connectedDeviceIP = null;
            
            // Stop ping timer
            _pingTimer.Stop();
            
            // Auto-disable Connected Mode - bypass the check since connection is lost
            Console.WriteLine($"[DEBUG] Disabling Connected Mode (bypass check)");
            _allowConnectedModeChange = false;
            cbConnectedMode.Checked = false;
            _allowConnectedModeChange = true;
            UpdateConnectedModeUI();
            
            // Cancel any ongoing downloads
            _downloadCancellationTokenSource?.Cancel();
            
            Console.WriteLine($"[DEBUG] Connection cleanup complete");
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

            Console.WriteLine($"[DEBUG] Incoming connection accepted");

            // Someone connected TO this device - enable Connected Mode
            _allowConnectedModeChange = false;
            cbConnectedMode.Checked = true;
            _allowConnectedModeChange = true;
            UpdateConnectedModeUI();
            
            // Start ping timer
            _pingTimer.Start();
            
            tsTextStatus.Text = "Device connected to you";
            
            Console.WriteLine($"[DEBUG] Connected Mode enabled for incoming connection");
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
    }
}
