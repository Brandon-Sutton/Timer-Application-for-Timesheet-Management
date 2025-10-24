using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TimerApplication
{
    internal static class Program
    {
        // DPI Awareness API calls for Windows 10 1703+
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set DPI awareness before any controls are created
            try
            {
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    // Try to set Per Monitor V2 DPI awareness for Windows 10 1703+
                    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
            }
            catch
            {
                // Fall back to framework's DPI handling
            }

            // Standard application configuration
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the application
            Application.Run(new Form1());
        }
    }
}