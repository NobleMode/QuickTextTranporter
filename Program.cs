using System.Diagnostics;

namespace QuickTextTranporter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check if .NET 8.0 runtime is available
            if (!CheckDotNetRuntime())
            {
                MessageBox.Show(
                    ".NET 8.0 Runtime is not installed!\n\n" +
                    "Please download and install .NET 8.0 Runtime from:\n" +
                    "https://dotnet.microsoft.com/download/dotnet/8.0\n\n" +
                    "Select: '.NET Desktop Runtime 8.0' for Windows\n\n" +
                    "Click OK to open the download page.",
                    "Missing .NET Runtime",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://dotnet.microsoft.com/download/dotnet/8.0",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // Ignore if browser doesn't open
                }

                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static bool CheckDotNetRuntime()
        {
            try
            {
                // Try to get the runtime version
                var version = Environment.Version;
                
                // Check if major version is 8 or higher
                if (version.Major >= 8)
                {
                    return true;
                }

                // Additional check using dotnet command
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--list-runtimes",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Check if .NET 8.0 runtime is listed
                return output.Contains("Microsoft.WindowsDesktop.App 8.") || 
                       output.Contains("Microsoft.NETCore.App 8.");
            }
            catch
            {
                // If we can't check, assume it's installed (we're running after all)
                return true;
            }
        }
    }
}