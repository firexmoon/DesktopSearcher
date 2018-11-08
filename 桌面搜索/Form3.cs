using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace 桌面搜索
{
    public partial class Form3 : Form
    {
        public static string filters;

        public struct FileItem
        {
            public string PathName;
            public DateTime LastWriteTime;
        };

        public static ArrayList FilesList = new ArrayList();
        public static ArrayList DirList = new ArrayList();

        public static ArrayList FSWatcher = new ArrayList();

        public static bool StateOK = false;

        Form1 SearchForm = new Form1();

        int HotKeyTimes = 0;
        Hotkey MyHotKey;
        int hotkeyID;

        public Form3()
        {
            InitializeComponent();


            MyHotKey = new Hotkey(this.Handle);
            hotkeyID = MyHotKey.RegisterHotkey(System.Windows.Forms.Keys.None, Hotkey.KeyFlags.MOD_CONTROL);
            if (hotkeyID == -1)
            {
                MessageBox.Show("注册热键失败！可能已经被占用。");
            }
            MyHotKey.OnHotkey += new HotkeyEventHandler(OnHotkey);
        }

        //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
        //参数为指定的目录
        public static int GetFiles(DirectoryInfo Dir)
        {
            FileItem fi = new FileItem();

            int resCount = 0;
            try
            {
                //查找子目录     
                foreach (DirectoryInfo d in Dir.GetDirectories())
                {
                    int r = GetFiles(d);
                    if (r > 0)
                    {
                        fi.PathName = d.FullName;
                        fi.LastWriteTime = d.LastWriteTime;
                        DirList.Add(fi);
                        resCount += r;
                    }
                }
                //查找文件
                foreach (FileInfo f in Dir.GetFiles("*.*"))
                {
                    if (filters.Equals("*.*") || filters.Contains(f.Extension.ToLower()))
                    {
                        fi.PathName = f.FullName;
                        fi.LastWriteTime = f.LastWriteTime;
                        FilesList.Add(fi);
                        resCount++;
                    }
                }
            }
            catch
            {
            }
            return resCount;
        }

        public static void p_Created(object sender, FileSystemEventArgs e)
        {
            FileItem fitem = new FileItem();
            string s = e.FullPath;
            FileInfo fi = new FileInfo(s);
            if (fi.Attributes == FileAttributes.Directory)
            {
                fitem.PathName = s;
                fitem.LastWriteTime = fi.LastWriteTime;
                DirList.Add(fitem);
            }
            else if (filters.Equals("*.*") || filters.Contains(fi.Extension.ToLower()))
            {
                fitem.PathName = s;
                fitem.LastWriteTime = fi.LastWriteTime;
                FilesList.Add(fitem);
            }
        }

        public static void p_Deleted(object sender, FileSystemEventArgs e)
        {
            string os = e.FullPath;

            foreach (FileItem fit in FilesList)
            {
                if (fit.PathName.Equals(os))
                {
                    FilesList.Remove(fit);
                    return;
                }
            }

            foreach (FileItem fit in DirList)
            {
                if (fit.PathName.Equals(os))
                {
                    DirList.Remove(fit);
                    return;
                }
            }

        }

        public static void p_Changed(object sender, FileSystemEventArgs e)
        {
            FileItem FI = new FileItem();
            string os = e.FullPath;
            FileInfo finfo = new FileInfo(os);
            if (finfo.Attributes == FileAttributes.Directory)
            {
                foreach (FileItem fi in DirList)
                {
                    if (fi.PathName.Equals(os) && fi.LastWriteTime != finfo.LastWriteTime)
                    {
                        DirList.Remove(fi);
                        FI.PathName = os;
                        FI.LastWriteTime = finfo.LastWriteTime;
                        DirList.Add(FI);
                        return;
                    }
                }
            }
            else if (filters.Equals("*.*") || filters.Contains(finfo.Extension.ToLower()))
            {
                foreach (FileItem fi in FilesList)
                {
                    if (fi.PathName.Equals(os) && fi.LastWriteTime != finfo.LastWriteTime)
                    {
                        FilesList.Remove(fi);
                        FI.PathName = os;
                        FI.LastWriteTime = finfo.LastWriteTime;
                        FilesList.Add(FI);
                        return;
                    }
                }
            }
        }

        public static void p_Renamed(object sender, RenamedEventArgs e)
        {
            string s = e.FullPath;
            string os = e.OldFullPath;
            FileItem fileitem = new FileItem();

            FileInfo fi = new FileInfo(s);
            if (fi.Attributes == FileAttributes.Directory)
            {
                foreach (FileItem fitem in DirList)
                {
                    if (fitem.PathName.Equals(os))
                    {
                        DirList.Remove(fitem);
                        break;
                    }
                }
                fileitem.PathName = s;
                fileitem.LastWriteTime = fi.LastWriteTime;
                DirList.Add(fileitem);
            }
            else
            {
                foreach (FileItem fitem in FilesList)
                {
                    if (fitem.PathName.Equals(os))
                    {
                        FilesList.Remove(fitem);
                        break;
                    }
                }
                if (filters.Equals("*.*") || filters.Contains(fi.Extension.ToLower()))
                {
                    fileitem.PathName = s;
                    fileitem.LastWriteTime = fi.LastWriteTime;
                    FilesList.Add(fileitem);
                }
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
            InitSearch();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

        }

        public static void InitSearch()
        {
            string str = Properties.Settings.Default.UserDirs;
            filters = Properties.Settings.Default.ExtNameFilter;

            StateOK = false;

            FilesList.Clear();
            DirList.Clear();

            foreach (FileSystemWatcher FSW in FSWatcher)
            {
                FSW.EnableRaisingEvents = false;
                FSW.Dispose();
            }
            FSWatcher.Clear();

            string[] dirs = str.Split(';');

            foreach (string d in dirs)
            {
                if (d == "")
                    continue;

                GetFiles(new DirectoryInfo(d));

                try
                {
                    FileSystemWatcher p = new FileSystemWatcher(d);
                    p.EnableRaisingEvents = true;
                    p.IncludeSubdirectories = true;
                    p.Created += new FileSystemEventHandler(p_Created);
                    p.Deleted += new FileSystemEventHandler(p_Deleted);
                    p.Renamed += new RenamedEventHandler(p_Renamed);
                    p.Changed += new FileSystemEventHandler(p_Changed);
                    p.Filter = "*.*";

                    FSWatcher.Add(p);
                }
                catch
                {
                }

            }

            StateOK = true;

        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SearchForm.ShowMe();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        public void OnHotkey(int HotkeyID)
        {
            if (HotkeyID == hotkeyID)
            {
                if (HotKeyTimes == 1)
                {
                    notifyIcon1_MouseDoubleClick(null, null);

                }
                else
                {
                    HotKeyTimes++;
                    HotKeyTimer.Enabled = true;
                }
            }

        }


        private void HotKeyTimer_Tick(object sender, EventArgs e)
        {
            HotKeyTimer.Enabled = false;
            HotKeyTimes = 0;
        }

        private void 选项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 FM = new Form2();
            FM.Show();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 AB = new AboutBox1();
            AB.ShowDialog();
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyHotKey.UnregisterHotkeys();
        }


    }
}
