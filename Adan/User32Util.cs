using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Adan
{
    public class User32Util
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        extern static IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]

        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, string lParam);
        [DllImport("User32.dll")]
        public static extern int GetWindowText(IntPtr WinHandle, StringBuilder Title, int size);
        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr WinHandle, StringBuilder Type, int size);
        [DllImport("User32.dll")]
        public static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);


        public delegate bool EnumWindowsProc(IntPtr WindowHandle, string num);
        private bool EnumChild(IntPtr handle, int num)
        {
            StringBuilder title = new StringBuilder();
            StringBuilder type = new StringBuilder();
            title.Length = 100;
            //type.Length = 100;

            GetWindowText(handle, title, 100);//取标题
            GetClassName(handle, type, 100);//取类型

            if (title.ToString() == "" && type.ToString() == typeName)
            {
                //mainHwnd = handle;
                list.Add(handle);
                //return false;
            }
            if (handle.ToInt32() == 201290)
            {
                list.Add(handle);
            }
            return true;
        }
        IntPtr mainHwnd = IntPtr.Zero;//登录窗口句柄
        List<IntPtr> list = new List<IntPtr>();
        string typeName = "#32770";//启动程序的窗口标题

        public struct WindowInfo
        {
            public IntPtr hWnd;
            public string szWindowName;
            public string szClassName;
        }
        public delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);

        const int WM_CLICKDown = 0x0201;
        const int WM_CLICKUp = 0x0202;
        const int WM_CLICK = 0x00F5;
        public WindowInfo[] GetAllDesktopWindows()
        {
            //用来保存窗口对象 列表
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows 
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                //WindowInfo wnd = new WindowInfo();
                //StringBuilder sb = new StringBuilder(256);

                //get hwnd 
                //wnd.hWnd = hWnd;

                //GetWindowText(hWnd, sb, sb.Capacity);
                //wnd.szWindowName = sb.ToString();

                //GetClassName(hWnd, sb, sb.Capacity);
                //wnd.szClassName = sb.ToString();
                if (hWnd.ToInt32() == 199426)
                {
                    IntPtr childHwnd = FindWindowEx(hWnd, IntPtr.Zero, null, "是(&Y)");   //获得按钮的句柄2166882
                    if (childHwnd != IntPtr.Zero)
                    {
                        SendMessage(childHwnd, WM_CLICK, 0, "0");
                        //SendMessage(childHwnd, WM_CLICKUp, 0, 0);//给子窗体上button发送鼠标点击消息，
                        return false;
                    }
                    //else
                    //{
                    //    MessageBox.Show("没有找到子窗口");
                    //}
                }
                //add it into list 
                //wndList.Add(wnd);
                return true;
            }, 0);

            return wndList.ToArray();
        }

        public static bool SpyStockForm()
        {
            //IntPtr weituoPtr = Process.GetProcessesByName("xiadan")[0].MainWindowHandle;

            IntPtr weituoPtr = FindWindow(null, "网上股票交易系统5.0");
            //IntPtr weituoquerenPtr = FindWindow(null, "是(&Y)");
            IntPtr childHwnd = FindWindowEx(weituoPtr, IntPtr.Zero, null, "是(&Y)");   //获得按钮的句柄

            if (childHwnd != IntPtr.Zero)
            {
                SendMessage(childHwnd, WM_CLICK, 0, "");     //发送点击按钮的消息  
                return true; 
            }
            else
            {
                MessageBox.Show("没有找到子窗口");
                return false;
            }
        }

        public void Spy()
        {
            DateTime t1 = DateTime.Now;
            Console.WriteLine(DateTime.Now.ToString());
            //IntPtr weituoPtr = FindWindow(null, "同花顺(v8.70.35) - 自选股");//"Afx:400000:b:10003:6:10491"
            //IntPtr weituoPtr = Process.GetProcessesByName("xiadan")[0].MainWindowHandle;
            IntPtr weituoPtr = FindWindow(null, "网上股票交易系统5.0");
            GetAllDesktopWindows();
            DateTime t2 = DateTime.Now;
            TimeSpan time = t2 - t1;
            Console.WriteLine(time.ToString());
            if (list.Count != 0)
            {
                MessageBox.Show("找到子窗口");
            }
            else
            {
                MessageBox.Show("没有找到子窗口");
            }
        }
    }
}
