using System.Diagnostics;
using System.Text.RegularExpressions;

namespace QuickTextTranporter
{
    public class CloudflareTunnelService
    {
        private Process? _process;
        private bool _isRunning;

        public event EventHandler<string>? TunnelUrlReady;
        public event EventHandler<string>? TunnelError;
        public event EventHandler? TunnelStopped;

        public bool IsRunning => _isRunning;
        public string? PublicUrl { get; private set; }
        public bool IsCloudflaredPresent => File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared.exe"));

        public async Task DownloadCloudflaredAsync(IProgress<int> progress)
        {
            var url = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared.exe");

            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(exePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var totalRead = 0L;
            var bytesRead = 0;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (canReportProgress)
                {
                    progress?.Report((int)((totalRead * 100) / totalBytes));
                }
            }
        }

        public void Start(int port)
        {
            if (_isRunning) return;

            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cloudflared.exe");
            if (!File.Exists(exePath))
            {
                TunnelError?.Invoke(this, "cloudflared.exe not found. Please download it and place it in the application folder.");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"tunnel --url http://localhost:{port}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _process = new Process { StartInfo = startInfo };
                _process.OutputDataReceived += (s, e) => ParseOutput(e.Data);
                _process.ErrorDataReceived += (s, e) => ParseOutput(e.Data);
                _process.EnableRaisingEvents = true;
                _process.Exited += (s, e) => Stop();

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _isRunning = true;
            }
            catch (Exception ex)
            {
                Stop();
                TunnelError?.Invoke(this, $"Failed to start tunnel: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit();
                }
            }
            catch { /* Ignore errors on stop */ }
            finally
            {
                _process?.Dispose();
                _process = null;
                TunnelStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ParseOutput(string? data)
        {
            if (string.IsNullOrEmpty(data)) return;

            // Look for the URL in the output
            // Example output: https://random-name.trycloudflare.com
            var match = Regex.Match(data, @"https://[a-zA-Z0-9-]+\.trycloudflare\.com");
            if (match.Success)
            {
                var url = match.Value;
                PublicUrl = url;
                TunnelUrlReady?.Invoke(this, url);
            }

            // Log errors if needed, but cloudflared outputs a lot of info to stderr
        }
    }
}
