using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace 桌面搜索
{
    static class Program
    {
            
        private static Mutex mutex;
        private static bool requestInitialOwnership = true;
        private static bool mutexWasCreated;


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            mutex = new Mutex(requestInitialOwnership, "FireXMoon_DesktopSearch_200913", out mutexWasCreated);
            if (requestInitialOwnership && mutexWasCreated)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form3());
            }
            else
            {
                MessageBox.Show("已经有一个 桌面快捷搜索 程序正在运行中！");
                System.Environment.Exit(0);
            }
        }
    }
}
