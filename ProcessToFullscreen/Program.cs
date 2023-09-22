using System.Configuration;
using System.Diagnostics;
using static ProcessToFullscreen.Fullscreen;

namespace ProcessToFullscreen
{
    internal static class Program
    {
        public enum Strategy
        {
            Fullscreen,
            Maximize,
            MaximizeAndFullscreen,
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var processName = ConfigurationManager.AppSettings.Get("ProcessName");
            var timeBetweenChecks = int.Parse(ConfigurationManager.AppSettings.Get("TimeBetweenChecks")!);
            var keepAlwaysInFullscreen = bool.Parse(ConfigurationManager.AppSettings.Get("KeepAlwaysInFullscreen")!.ToString());
            var keepAlwaysMaximized = bool.Parse(ConfigurationManager.AppSettings.Get("KeepAlwaysMaximized")!.ToString());
            var strategy = Enum.Parse<Strategy>(ConfigurationManager.AppSettings.Get("Strategy")!.ToString());
            var maximizeMethod = Enum.Parse<MaximizeMethod>(ConfigurationManager.AppSettings.Get("MaximizeMethod")!.ToString());

            var key = Enum.Parse<Keys>(ConfigurationManager.AppSettings.Get("KeyToPutInFullscreen")!);


            var processAlreadySetInFullscreen = new List<Process>();
            var processAlreadySetToMaximized = new List<Process>();

            while (true)
            {
                var allProcesses = Process.GetProcesses();

                var processes = allProcesses.Where(process => string.Equals(process.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase));

                foreach (var process in processes)
                {
                    if (strategy == Strategy.Maximize || strategy == Strategy.MaximizeAndFullscreen)
                    {
                        if (keepAlwaysMaximized || !processAlreadySetToMaximized.Any(p => p.Id == process.Id))
                        {
                            Fullscreen.SetForegroundWindow(process.MainWindowHandle);
                            var state = Fullscreen.GetState(process.MainWindowHandle);
                            if (state != ProcessWindowStyle.Maximized)
                            {
                                try
                                {
                                    NativeMethods.BlockInput(true);
                                    Fullscreen.MaximizeWindow(process.MainWindowHandle, maximizeMethod);
                                    Thread.Sleep(2000);
                                    bool isMaximizedCheck = Fullscreen.GetState(process.MainWindowHandle) == ProcessWindowStyle.Maximized;
                                    if (isMaximizedCheck)
                                    {
                                        processAlreadySetToMaximized.Add(process);
                                    }
                                }
                                finally
                                {
                                    NativeMethods.BlockInput(false);
                                }
                            }
                        }
                    }

                    //Thread.Sleep(20000);

                    if (strategy == Strategy.Fullscreen || strategy == Strategy.MaximizeAndFullscreen)
                    {
                        if (keepAlwaysInFullscreen || !processAlreadySetInFullscreen.Any(p => p.Id == process.Id))
                        {
                            Fullscreen.SetForegroundWindow(process.MainWindowHandle);
                            var screen = Screen.FromHandle(process.MainWindowHandle);
                            bool isFullscreen = Fullscreen.IsForegroundFullScreen(screen);

                            if (!isFullscreen)
                            {
                                try
                                {
                                    NativeMethods.BlockInput(true);
                                    Fullscreen.SetForegroundWindow(process.MainWindowHandle);
                                    KeyboardSend.Send(key);
                                    Thread.Sleep(100);
                                    Fullscreen.SetForegroundWindow(process.MainWindowHandle);
                                    screen = Screen.FromHandle(process.MainWindowHandle);
                                    bool isInFullscreenCheck = Fullscreen.IsForegroundFullScreen(screen);
                                    if (isInFullscreenCheck)
                                    {
                                        processAlreadySetInFullscreen.Add(process);
                                    }
                                }
                                finally
                                {
                                    NativeMethods.BlockInput(false);
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(timeBetweenChecks);
            }
        }
    }
}