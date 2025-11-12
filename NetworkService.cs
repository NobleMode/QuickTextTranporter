using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace QuickTextTranporter
{
    public class NetworkService : IDisposable
    {
        private const int DISCOVERY_PORT = 45678;
        private const int COMMUNICATION_PORT = 45679;
        private const string DISCOVERY_MESSAGE = "QUICK_TEXT_TRANSPORT_DISCOVER";
        private const string DISCOVERY_RESPONSE = "QUICK_TEXT_TRANSPORT_RESPONSE";

        private UdpClient? _discoveryListener;
        private TcpListener? _tcpListener;
        private TcpClient? _connectedClient;
        private NetworkStream? _networkStream;
        private CancellationTokenSource? _cancellationTokenSource;

        public string LocalDeviceName { get; set; }
        public event EventHandler<DeviceDiscoveredEventArgs>? DeviceDiscovered;
        public event EventHandler<TextReceivedEventArgs>? TextReceived;
        public event EventHandler<FileListReceivedEventArgs>? FileListReceived;
        public event EventHandler<FileRequestEventArgs>? FileRequested;
        public event EventHandler<FileDataReceivedEventArgs>? FileDataReceived;
        public event EventHandler? ConnectionLost;
        public event EventHandler? IncomingConnectionAccepted;

        public bool IsConnected => _connectedClient?.Connected ?? false;
        private DateTime _lastPongReceived = DateTime.Now;

        public NetworkService()
        {
            LocalDeviceName = Environment.MachineName;
        }

        public async Task StartListeningAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Start UDP discovery listener
            _ = Task.Run(() => ListenForDiscoveryAsync(_cancellationTokenSource.Token));

            // Start TCP listener for connections
            _ = Task.Run(() => ListenForConnectionsAsync(_cancellationTokenSource.Token));

            await Task.CompletedTask;
        }

        private async Task ListenForDiscoveryAsync(CancellationToken token)
        {
            try
            {
                _discoveryListener = new UdpClient(DISCOVERY_PORT);
                _discoveryListener.EnableBroadcast = true;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _discoveryListener.ReceiveAsync(token);
                        var message = Encoding.UTF8.GetString(result.Buffer);

                        if (message == DISCOVERY_MESSAGE)
                        {
                            // Respond to discovery request
                            var response = Encoding.UTF8.GetBytes($"{DISCOVERY_RESPONSE}|{LocalDeviceName}");
                            await _discoveryListener.SendAsync(response, response.Length, result.RemoteEndPoint);
                        }
                        else if (message.StartsWith(DISCOVERY_RESPONSE))
                        {
                            // Device discovered
                            var parts = message.Split('|');
                            if (parts.Length > 1)
                            {
                                DeviceDiscovered?.Invoke(this, new DeviceDiscoveredEventArgs
                                {
                                    DeviceName = parts[1],
                                    IPAddress = result.RemoteEndPoint.Address.ToString()
                                });
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Continue listening on error
                    }
                }
            }
            catch (Exception)
            {
                // Handle listener creation error
            }
        }

        private async Task ListenForConnectionsAsync(CancellationToken token)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, COMMUNICATION_PORT);
                _tcpListener.Start();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await _tcpListener.AcceptTcpClientAsync(token);
                        
                        // If we already have a connection, close the new one
                        if (_connectedClient != null && _connectedClient.Connected)
                        {
                            client.Close();
                            continue;
                        }

                        _connectedClient = client;
                        _networkStream = client.GetStream();

                        // Notify that someone connected TO us
                        IncomingConnectionAccepted?.Invoke(this, EventArgs.Empty);

                        // Start receiving messages
                        _ = Task.Run(() => ReceiveMessagesAsync(token));
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Continue listening
                    }
                }
            }
            catch (Exception)
            {
                // Handle listener creation error
            }
        }

        public async Task<List<DiscoveredDevice>> DiscoverDevicesAsync(int timeoutMs = 2000)
        {
            var devices = new List<DiscoveredDevice>();
            var discoveredIPs = new HashSet<string>();

            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            var discoveryMessage = Encoding.UTF8.GetBytes(DISCOVERY_MESSAGE);
            
            // Get all local network interfaces and broadcast to each subnet
            var localIPs = GetAllLocalIPAddresses();
            
            foreach (var localIP in localIPs)
            {
                try
                {
                    // Broadcast to the specific subnet
                    var broadcastIP = GetBroadcastAddress(localIP);
                    var broadcastEndpoint = new IPEndPoint(broadcastIP, DISCOVERY_PORT);
                    await udpClient.SendAsync(discoveryMessage, discoveryMessage.Length, broadcastEndpoint);
                }
                catch
                {
                    // Continue with other interfaces
                }
            }

            // Also send to general broadcast
            var generalBroadcast = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);
            await udpClient.SendAsync(discoveryMessage, discoveryMessage.Length, generalBroadcast);

            var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync(cts.Token);
                        var message = Encoding.UTF8.GetString(result.Buffer);

                        if (message.StartsWith(DISCOVERY_RESPONSE))
                        {
                            var parts = message.Split('|');
                            if (parts.Length > 1)
                            {
                                var ip = result.RemoteEndPoint.Address.ToString();
                                
                                // Don't add duplicate IPs or localhost
                                if (!discoveredIPs.Contains(ip) && !IsLocalIP(ip))
                                {
                                    discoveredIPs.Add(ip);
                                    devices.Add(new DiscoveredDevice
                                    {
                                        DeviceName = parts[1],
                                        IPAddress = ip
                                    });
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // Timeout or error
            }

            return devices;
        }

        private List<IPAddress> GetAllLocalIPAddresses()
        {
            var addresses = new List<IPAddress>();
            
            try
            {
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                
                foreach (var addr in hostEntry.AddressList)
                {
                    // Only get IPv4 addresses that are not loopback
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                        !IPAddress.IsLoopback(addr))
                    {
                        addresses.Add(addr);
                    }
                }
            }
            catch
            {
                // If we can't get addresses, return empty list
            }

            return addresses;
        }

        private IPAddress GetBroadcastAddress(IPAddress address)
        {
            try
            {
                // Get the subnet mask for this interface
                var addressBytes = address.GetAddressBytes();
                
                // Assume /24 subnet (255.255.255.0) - most common for home networks
                // This means broadcast is x.x.x.255
                addressBytes[3] = 255;
                
                return new IPAddress(addressBytes);
            }
            catch
            {
                return IPAddress.Broadcast;
            }
        }

        private bool IsLocalIP(string ip)
        {
            try
            {
                var targetIP = IPAddress.Parse(ip);
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                
                return hostEntry.AddressList.Any(addr => addr.Equals(targetIP));
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ConnectToDeviceAsync(string ipAddress)
        {
            try
            {
                // Disconnect existing connection
                DisconnectFromDevice();

                _connectedClient = new TcpClient();
                await _connectedClient.ConnectAsync(ipAddress, COMMUNICATION_PORT);
                _networkStream = _connectedClient.GetStream();

                // Start receiving messages
                _cancellationTokenSource ??= new CancellationTokenSource();
                _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DisconnectFromDevice()
        {
            _networkStream?.Close();
            _networkStream = null;
            _connectedClient?.Close();
            _connectedClient = null;
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            if (_networkStream == null) return;

            try
            {
                while (!token.IsCancellationRequested && _connectedClient?.Connected == true)
                {
                    // Read length prefix (4 bytes)
                    var lengthBuffer = new byte[4];
                    int bytesRead = 0;
                    
                    while (bytesRead < 4)
                    {
                        int read = await _networkStream.ReadAsync(lengthBuffer, bytesRead, 4 - bytesRead, token);
                        if (read == 0)
                        {
                            // Connection closed
                            ConnectionLost?.Invoke(this, EventArgs.Empty);
                            return;
                        }
                        bytesRead += read;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // Read the actual message
                    var messageBuffer = new byte[messageLength];
                    bytesRead = 0;

                    while (bytesRead < messageLength)
                    {
                        int read = await _networkStream.ReadAsync(messageBuffer, bytesRead, messageLength - bytesRead, token);
                        if (read == 0)
                        {
                            // Connection closed
                            ConnectionLost?.Invoke(this, EventArgs.Empty);
                            return;
                        }
                        bytesRead += read;
                    }

                    var message = Encoding.UTF8.GetString(messageBuffer, 0, messageLength);
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<NetworkMessage>(message);
                if (data == null) return;

                switch (data.Type)
                {
                    case MessageType.Text:
                        TextReceived?.Invoke(this, new TextReceivedEventArgs { Text = data.Content ?? "" });
                        break;

                    case MessageType.FileList:
                        var files = JsonSerializer.Deserialize<List<FileInfo>>(data.Content ?? "[]");
                        FileListReceived?.Invoke(this, new FileListReceivedEventArgs { Files = files ?? new List<FileInfo>() });
                        break;

                    case MessageType.FileRequest:
                        FileRequested?.Invoke(this, new FileRequestEventArgs { FileName = data.Content ?? "" });
                        break;

                    case MessageType.FileData:
                        HandleFileData(data.Content ?? "");
                        break;

                    case MessageType.Ping:
                        // Respond with pong
                        await SendPongAsync();
                        break;

                    case MessageType.Pong:
                        // Update last pong time
                        _lastPongReceived = DateTime.Now;
                        break;
                }
            }
            catch (Exception)
            {
                // Handle parsing error
            }
        }

        private void HandleFileData(string content)
        {
            try
            {
                var fileData = JsonSerializer.Deserialize<FileTransferData>(content);
                if (fileData != null)
                {
                    var bytes = Convert.FromBase64String(fileData.Data ?? "");
                    
                    FileDataReceived?.Invoke(this, new FileDataReceivedEventArgs 
                    { 
                        FileName = fileData.FileName ?? "",
                        Data = bytes
                    });
                }
            }
            catch (Exception)
            {
                // Silently handle file data errors
            }
        }

        public async Task SendTextAsync(string text)
        {
            if (!IsConnected) return;

            try
            {
                var message = new NetworkMessage
                {
                    Type = MessageType.Text,
                    Content = text
                };

                await SendMessageAsync(message);
            }
            catch (Exception)
            {
                // Handle send error
            }
        }

        public async Task SendFileListAsync(List<FileInfo> files)
        {
            if (!IsConnected) return;

            try
            {
                var message = new NetworkMessage
                {
                    Type = MessageType.FileList,
                    Content = JsonSerializer.Serialize(files)
                };

                await SendMessageAsync(message);
            }
            catch (Exception)
            {
                // Handle send error
            }
        }

        public async Task RequestFileAsync(string fileName, string savePath, IProgress<int>? progress, CancellationToken cancellationToken)
        {
            if (!IsConnected) return;

            try
            {
                var message = new NetworkMessage
                {
                    Type = MessageType.FileRequest,
                    Content = fileName
                };

                // Set up one-time handler for file data
                EventHandler<FileDataReceivedEventArgs>? handler = null;
                var tcs = new TaskCompletionSource<bool>();

                handler = async (s, e) =>
                {
                    if (e.FileName == fileName)
                    {
                        try
                        {
                            progress?.Report(10);
                            
                            // Save the file
                            await File.WriteAllBytesAsync(savePath, e.Data, cancellationToken);
                            
                            // Update progress
                            for (int i = 20; i <= 100; i += 20)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    tcs.SetCanceled();
                                    return;
                                }
                                progress?.Report(i);
                                await Task.Delay(100, cancellationToken);
                            }
                            
                            progress?.Report(100);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                        finally
                        {
                            if (handler != null)
                                FileDataReceived -= handler;
                        }
                    }
                };

                FileDataReceived += handler;

                // Send the request
                await SendMessageAsync(message);

                // Wait for file with timeout (30 seconds)
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                using (linkedCts.Token.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SendPingAsync()
        {
            if (!IsConnected) return;

            try
            {
                var message = new NetworkMessage
                {
                    Type = MessageType.Ping,
                    Content = ""
                };

                await SendMessageAsync(message);
            }
            catch (Exception)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task SendPongAsync()
        {
            if (!IsConnected) return;

            try
            {
                var message = new NetworkMessage
                {
                    Type = MessageType.Pong,
                    Content = ""
                };

                await SendMessageAsync(message);
            }
            catch (Exception)
            {
                // Ignore pong errors
            }
        }

        public async Task SendFileAsync(string filePath, string fileName)
        {
            if (!IsConnected) return;

            try
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                
                var fileData = new FileTransferData
                {
                    FileName = fileName,
                    Data = Convert.ToBase64String(bytes)
                };

                var message = new NetworkMessage
                {
                    Type = MessageType.FileData,
                    Content = JsonSerializer.Serialize(fileData)
                };

                await SendMessageAsync(message);
            }
            catch (Exception)
            {
                // Silently handle file send errors
            }
        }

        private async Task SendMessageAsync(NetworkMessage message)
        {
            if (_networkStream == null || !IsConnected) return;

            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Send length prefix (4 bytes)
                var lengthBytes = BitConverter.GetBytes(bytes.Length);
                await _networkStream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

                // Send actual message
                await _networkStream.WriteAsync(bytes, 0, bytes.Length);
                await _networkStream.FlushAsync();
            }
            catch (Exception)
            {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _networkStream?.Close();
            _connectedClient?.Close();
            _discoveryListener?.Close();
            _tcpListener?.Stop();
            _cancellationTokenSource?.Dispose();
        }
    }

    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string? Content { get; set; }
    }

    public enum MessageType
    {
        Text,
        FileList,
        FileRequest,
        FileData,
        Ping,
        Pong
    }

    public class FileInfo
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
    }

    public class FileTransferData
    {
        public string? FileName { get; set; }
        public string? Data { get; set; }
    }

    public class DiscoveredDevice
    {
        public string DeviceName { get; set; } = "";
        public string IPAddress { get; set; } = "";

        public override string ToString() => DeviceName;
    }

    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public string DeviceName { get; set; } = "";
        public string IPAddress { get; set; } = "";
    }

    public class TextReceivedEventArgs : EventArgs
    {
        public string Text { get; set; } = "";
    }

    public class FileListReceivedEventArgs : EventArgs
    {
        public List<FileInfo> Files { get; set; } = new();
    }

    public class FileRequestEventArgs : EventArgs
    {
        public string FileName { get; set; } = "";
    }

    public class FileDataReceivedEventArgs : EventArgs
    {
        public string FileName { get; set; } = "";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
