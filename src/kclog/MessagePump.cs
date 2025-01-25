using System;
using System.Runtime.InteropServices;

// See https://github.com/Indieteur/GlobalHooks?tab=readme-ov-file for information on how the hook works
namespace kclog
{
    static class MessagePump
    {
        [Serializable]
        public struct MSG
        {
            public IntPtr hwnd;
            public IntPtr lParam;
            public int message;
            public int pt_x;
            public int pt_y;
            public int time;
            public IntPtr wParam;
        }

        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        public static void WaitForMessages()
        {
            while ((!GetMessage(out MSG msg, IntPtr.Zero, 0, 0)))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
    }
}