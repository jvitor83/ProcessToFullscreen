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
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);


        public enum MaximizeMethod 
        {
            Normal,
            MouseTitleDoubleClick
        }

        public static void MaximizeWindow(IntPtr hWnd, MaximizeMethod method = MaximizeMethod.Normal)
        {
            if (method == MaximizeMethod.Normal)
            {
                ShowWindow(hWnd, SW_SHOWMAXIMIZED);
            }
            else
            {
                //WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                //GetWindowPlacement(hWnd, ref wp);

                //// Maximize window if it is in a normal state
                //// You can also do the reverse by simply checking and setting 
                //// the value of wp.showCmd
                //if (wp.showCmd == 1)
                //{
                //    wp.showCmd = 3;
                //}
                //SetWindowPlacement(hWnd, ref wp);

                //Thread.Sleep(1000);
                var screen = Screen.FromHandle(hWnd);

                RECT rect = new RECT();
                GetWindowRect(new HandleRef(null, hWnd), ref rect);

                

                //SetWindowPos(hWnd, Process.GetCurrentProcess().MainWindowHandle,
                //    0, 0, screen.Bounds.Width,
                //    screen.Bounds.Height, 1);
                Thread.Sleep(500);
                Cursor.Position = new Point(rect.left + 50, rect.top + 15);
                Mouse.MouseEvent(Mouse.MouseEventFlags.LeftDown);
                Mouse.MouseEvent(Mouse.MouseEventFlags.LeftUp);
                Mouse.MouseEvent(Mouse.MouseEventFlags.LeftDown);
                Mouse.MouseEvent(Mouse.MouseEventFlags.LeftUp);
                //MoveWindow(hWnd, 0,0, screen.Bounds.Width-1, screen.Bounds.Height-1, false);

            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
       int x, int y, int width, int height, uint uFlags);

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





        #region Display Resolution

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }


        public static double GetWindowsScreenScalingFactor(bool percentage = true)
        {
            //Create Graphics object from the current windows handle
            Graphics GraphicsObject = Graphics.FromHwnd(IntPtr.Zero);
            //Get Handle to the device context associated with this Graphics object
            IntPtr DeviceContextHandle = GraphicsObject.GetHdc();
            //Call GetDeviceCaps with the Handle to retrieve the Screen Height
            int LogicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.DESKTOPVERTRES);
            //Divide the Screen Heights to get the scaling factor and round it to two decimals
            double ScreenScalingFactor = Math.Round(PhysicalScreenHeight / (double)LogicalScreenHeight, 2);
            //If requested as percentage - convert it
            if (percentage)
            {
                ScreenScalingFactor *= 100.0;
            }
            //Release the Handle and Dispose of the GraphicsObject object
            GraphicsObject.ReleaseHdc(DeviceContextHandle);
            GraphicsObject.Dispose();
            //Return the Scaling Factor
            return ScreenScalingFactor;
        }

        public static Size GetDisplayResolution(Screen screen)
        {
            var sf = GetWindowsScreenScalingFactor(false);
            var screenWidth = screen.Bounds.Width * sf;
            var screenHeight = screen.Bounds.Height * sf;
            return new Size((int)screenWidth, (int)screenHeight);
        }

        #endregion



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
