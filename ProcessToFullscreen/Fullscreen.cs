using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.DataFormats;

namespace ProcessToFullscreen
{
    public static class Fullscreen
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public static ProcessWindowStyle GetState(IntPtr windowHandle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, ref placement);
            switch (placement.showCmd)
            {
                case 1:
                    return ProcessWindowStyle.Normal;
                    break;
                case 2:
                    return ProcessWindowStyle.Minimized;
                    break;
                case 3:
                    return ProcessWindowStyle.Maximized;
                    break;
                default: 
                    return ProcessWindowStyle.Hidden;
            }
        }



        // Pinvoke declaration for ShowWindow
        private const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void MaximizeWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOWMAXIMIZED);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static bool IsForegroundFullScreen()
        {
            return IsForegroundFullScreen(null);
        }

        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static bool IsForegroundFullScreen(System.Windows.Forms.Screen screen)
        {

            if (screen == null)
            {
                screen = System.Windows.Forms.Screen.PrimaryScreen;
            }
            RECT rect = new RECT();
            IntPtr hWnd = (IntPtr)GetForegroundWindow();


            GetWindowRect(new HandleRef(null, hWnd), ref rect);

            /* in case you want the process name:
            uint procId = 0;
            GetWindowThreadProcessId(hWnd, out procId);
            var proc = System.Diagnostics.Process.GetProcessById((int)procId);
            Console.WriteLine(proc.ProcessName);
            */


            if (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top))
            {
                Console.WriteLine("Fullscreen!");
                return true;
            }
            else
            {
                Console.WriteLine("Nope, :-(");
                return false;
            }


        }
    }
}
