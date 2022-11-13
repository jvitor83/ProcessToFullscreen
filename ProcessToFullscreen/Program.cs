using System.Configuration;
using System.Diagnostics;

namespace ProcessToFullscreen
{
    internal static class Program
    {
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

            var maximize = bool.Parse(ConfigurationManager.AppSettings.Get("Maximize")!.ToString());
            var key = Enum.Parse<Keys>(ConfigurationManager.AppSettings.Get("KeyToPutInFullscreen")!);


            var processAlreadySetInFullscreen = new List<Process>();
            var processAlreadySetToMaximized = new List<Process>();

            while (true)
            {
                var allProcesses = Process.GetProcesses();

                var processes = allProcesses.Where(process => string.Equals(process.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase));

                foreach (var process in processes)
                {
                    if (maximize)
                    {
                        if (keepAlwaysMaximized || !processAlreadySetToMaximized.Any(p => p.Id == process.Id))
                        {
                            var state = Fullscreen.GetState(process.MainWindowHandle);
                            if (state != ProcessWindowStyle.Maximized)
                            {
                                Fullscreen.MaximizeWindow(process.MainWindowHandle);
                                Thread.Sleep(2000);
                                bool isMaximizedCheck = Fullscreen.GetState(process.MainWindowHandle) == ProcessWindowStyle.Maximized;
                                if (isMaximizedCheck)
                                {
                                    processAlreadySetToMaximized.Add(process);
                                }
                            }
                        }
                    }

                    if (keepAlwaysInFullscreen || !processAlreadySetInFullscreen.Any(p => p.Id == process.Id))
                    {
                        Fullscreen.SetForegroundWindow(process.MainWindowHandle);
                        var screen = Screen.FromHandle(process.MainWindowHandle);
                        bool isFullscreen = Fullscreen.IsForegroundFullScreen(screen);
                        if (!isFullscreen)
                        {
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
                    }
                }

                Thread.Sleep(timeBetweenChecks);
            }
        }
    }
}