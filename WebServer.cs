using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.IO;

namespace QuickTextTranporter
{
    public class WebServer
    {
        private HttpListener? _listener;
        private Thread? _listenerThread;
        private bool _isRunning;
        private readonly int _port;

        // Store client state by IP
        private readonly ConcurrentDictionary<string, WebClientState> _clients = new();

        private System.Timers.Timer? _cleanupTimer;

        public event EventHandler<string>? ClientConnected;
        public event EventHandler<string>? ClientDisconnected;
        public event EventHandler<string>? ClientUpdated; // Text or activity update
        public event EventHandler<(string Ip, string Text)>? TextReceived;

        public string HostText { get; set; } = "";
        public List<FileInfo> HostFiles { get; set; } = new();

        public WebServer(int port = 45680)
        {
            _port = port;
        }

        public bool IsRunning => _isRunning;
        public int ClientCount => _clients.Count;
        public IEnumerable<string> ActiveClients => _clients.Keys;
        public IEnumerable<WebClientState> Clients => _clients.Values;

        public WebClientState? GetClientState(string ip)
        {
            _clients.TryGetValue(ip, out var state);
            return state;
        }


        public void Start()
        {
            if (_isRunning) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{_port}/");
            try
            {
                _listener.Start();
                _isRunning = true;
                _listenerThread = new Thread(ListenLoop);
                _listenerThread.Start();

                _cleanupTimer = new System.Timers.Timer(5000); // Check every 5 seconds
                _cleanupTimer.Elapsed += CleanupInactiveClients;
                _cleanupTimer.AutoReset = true;
                _cleanupTimer.Start();
            }
            catch (Exception)
            {
                _listener = null;
                throw;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            _cleanupTimer = null;

            try
            {
                _listener?.Stop();
            }
            catch { /* Ignore errors on stop */ }
            _listener = null;
            _clients.Clear(); // Clear clients on stop
        }

        private void CleanupInactiveClients(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(15); // Disconnect after 15 seconds of inactivity

            foreach (var client in _clients)
            {
                if (now - client.Value.LastSeen > timeout)
                {
                    if (_clients.TryRemove(client.Key, out _))
                    {
                        ClientDisconnected?.Invoke(this, client.Key);
                    }
                }
            }
        }

        private void ListenLoop()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(HandleRequest, context);
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                    break;
                }
                catch (Exception)
                {
                    // Ignore other errors
                }
            }
        }

        private void HandleRequest(object? state)
        {
            if (state is not HttpListenerContext context) return;

            var request = context.Request;
            var response = context.Response;

            // Determine client IP, handling proxies (Ngrok, Cloudflare)
            // Determine client IP, handling proxies (Ngrok, Cloudflare)
            string clientIp = request.RemoteEndPoint.Address.ToString();

            if (request.Headers["X-Forwarded-For"] != null)
            {
                // X-Forwarded-For can be a comma-separated list of IPs
                var forwardedFor = request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    clientIp = forwardedFor.Split(',')[0].Trim();
                }
            }
            else if (request.Headers["X-Real-IP"] != null)
            {
                clientIp = request.Headers["X-Real-IP"] ?? clientIp;
            }
            else if (request.Headers["CF-Connecting-IP"] != null) // Cloudflare specific
            {
                clientIp = request.Headers["CF-Connecting-IP"] ?? clientIp;
            }

            // Determine unique client identifier (Device ID or IP)
            string clientId = clientIp;
            string deviceId = "";
            if (request.Headers["X-Device-ID"] != null)
            {
                deviceId = request.Headers["X-Device-ID"] ?? "";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    clientId = deviceId;
                }
            }

            // Update or create client state
            if (!_clients.ContainsKey(clientId))
            {
                _clients.TryAdd(clientId, new WebClientState { IpAddress = clientIp, DeviceId = deviceId });
                ClientConnected?.Invoke(this, clientId);
            }

            var client = _clients[clientId];
            // Ensure IP is up to date (in case device roaming)
            client.IpAddress = clientIp;
            if (!string.IsNullOrEmpty(deviceId)) client.DeviceId = deviceId;
            client.LastSeen = DateTime.Now;

            try
            {
                if (request.HttpMethod == "GET")
                {
                    if (request.Url?.AbsolutePath == "/")
                    {
                        ServeIndexPage(response);
                    }
                    else if (request.Url?.AbsolutePath == "/init")
                    {
                        ServeInitData(response, clientIp, client);
                    }
                    else if (request.Url?.AbsolutePath == "/poll")
                    {
                        ServePollData(response, client);
                    }
                    else if (request.Url?.AbsolutePath.StartsWith("/download/") == true)
                    {
                        var fileName = request.Url.AbsolutePath.Substring("/download/".Length);
                        ServeFile(response, fileName);
                    }
                    else
                    {
                        // Serve static files from Assets
                        ServeStaticFile(response, request.Url?.AbsolutePath ?? "");
                    }
                }
                else if (request.HttpMethod == "POST")
                {
                    if (request.Url?.AbsolutePath == "/text")
                    {
                        HandleTextPost(request, client);
                    }
                    else if (request.Url?.AbsolutePath == "/upload")
                    {
                        HandleFileUpload(request, client, response);
                        return; // HandleFileUpload handles the response
                    }
                }
                else if (request.HttpMethod == "DELETE")
                {
                    if (request.Url?.AbsolutePath == "/files")
                    {
                        HandleFileDelete(request, client, response);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                response.Close();
            }
        }

        private void HandleFileDelete(HttpListenerRequest request, WebClientState client, HttpListenerResponse response)
        {
            try
            {
                var filename = request.QueryString["name"];
                if (string.IsNullOrEmpty(filename))
                {
                    response.StatusCode = 400;
                    return;
                }

                // Security check: prevent directory traversal
                if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                {
                    response.StatusCode = 400;
                    return;
                }

                var fileToDelete = client.UploadedFiles.FirstOrDefault(f => f.Name == filename);
                if (fileToDelete != null)
                {
                    if (fileToDelete.Exists)
                    {
                        fileToDelete.Delete();
                    }
                    client.UploadedFiles.Remove(fileToDelete);

                    // Notify UI
                    ClientUpdated?.Invoke(this, client.IpAddress);

                    response.StatusCode = 200;
                }
                else
                {
                    response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
        }

        private void ServeStaticFile(HttpListenerResponse response, string path)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", path.TrimStart('/'));
            if (File.Exists(filePath))
            {
                var extension = Path.GetExtension(filePath).ToLower();
                response.ContentType = extension switch
                {
                    ".html" => "text/html",
                    ".css" => "text/css",
                    ".js" => "text/javascript",
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    _ => "application/octet-stream"
                };

                var bytes = File.ReadAllBytes(filePath);
                response.ContentLength64 = bytes.Length;
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        private void ServeIndexPage(HttpListenerResponse response)
        {
            var indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "index.html");
            if (File.Exists(indexPath))
            {
                var bytes = File.ReadAllBytes(indexPath);
                response.ContentType = "text/html";
                response.ContentLength64 = bytes.Length;
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        private void ServeInitData(HttpListenerResponse response, string ip, WebClientState client)
        {
            var data = new
            {
                ip = ip,
                deviceId = ip.GetHashCode().ToString("X"),
                hostText = HostText,
                clientText = client.Text
            };

            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private void HandleFileUpload(HttpListenerRequest request, WebClientState client, HttpListenerResponse response)
        {
            try
            {
                // Simple multipart parser or just save body if single file
                // For robustness, we'll assume the client sends a proper multipart form
                // But implementing a full multipart parser from scratch is verbose.
                // Let's use a simplified approach: Save the stream to a temp file and let the user know.
                // OR, since we control the client, we can just POST the raw binary and use a header for filename.
                // Let's use the raw binary approach for simplicity and reliability without external libs.

                var filename = request.Headers["X-File-Name"];
                if (string.IsNullOrEmpty(filename))
                {
                    response.StatusCode = 400;
                    return;
                }

                filename = WebUtility.UrlDecode(filename);
                var uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads", "WebUploads", client.IpAddress);
                Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, filename);
                using (var fs = File.Create(filePath))
                {
                    request.InputStream.CopyTo(fs);
                }

                var fileInfo = new System.IO.FileInfo(filePath);
                client.UploadedFiles.Add(fileInfo);

                // Notify UI
                ClientUpdated?.Invoke(this, client.IpAddress);

                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
        }

        private void ServePollData(HttpListenerResponse response, WebClientState client)
        {
            var data = new
            {
                hostText = HostText,
                files = HostFiles.Select(f => new { f.FileName, f.FileSize }),
                uploadedFiles = client.UploadedFiles.Select(f => new { FileName = f.Name, FileSize = f.Length })
            };

            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private void HandleTextPost(HttpListenerRequest request, WebClientState client)
        {
            using var reader = new StreamReader(request.InputStream);
            var text = reader.ReadToEnd();

            if (client.Text != text)
            {
                client.Text = text;
                TextReceived?.Invoke(this, (client.IpAddress, text));
                ClientUpdated?.Invoke(this, client.IpAddress);
            }
        }

        private void ServeFile(HttpListenerResponse response, string fileName)
        {
            var fileNameDecoded = WebUtility.UrlDecode(fileName);
            var file = HostFiles.FirstOrDefault(f => f.FileName == fileNameDecoded);

            if (file != null && File.Exists(file.FilePath))
            {
                response.ContentType = "application/octet-stream";
                response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                using var fs = File.OpenRead(file.FilePath);
                response.ContentLength64 = fs.Length;
                fs.CopyTo(response.OutputStream);
            }
            else
            {
                response.StatusCode = 404;
            }
        }
    }

    public class WebClientState
    {
        public string IpAddress { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime LastSeen { get; set; }
        public List<System.IO.FileInfo> UploadedFiles { get; set; } = new();
    }
}
