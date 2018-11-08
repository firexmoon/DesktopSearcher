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
    public partial class Form1 : Form
    {
        ArrayList StrQueue = new ArrayList();
        int pStrQueue;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void ShowMe()
        {
            Form1_Resize(null, null);
            this.Show();

            listBox1.Items.Clear();
            label1.Text = "";
            textBox1.Text = "";
            pStrQueue = StrQueue.Count-1;

            this.TopMost = true;
            this.TopMost = false;

            this.Activate();
            textBox1.Focus();
        }

        //比较函数
        private class myFileInfoCompare : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return ((Form3.FileItem)y).LastWriteTime.CompareTo(((Form3.FileItem)x).LastWriteTime);
            }
        }
        private void SearchFiles()
        {
            listBox1.Items.Clear();

            ArrayList res_Dir = new ArrayList();
            ArrayList res_File = new ArrayList();

            bool FindinPath = checkBox1.Checked;
            string findstr = textBox1.Text.Trim().ToLower();

            if (findstr == "")
            {
                label1.Text = "";
                return;
            }
            string[] fstr = findstr.Split(' ');

            int count = 0;
            bool res;
            string tem;

            foreach (Form3.FileItem fi in Form3.DirList)
            {
                if (FindinPath)
                {
                    tem = fi.PathName.ToLower();
                }
                else
                {
                    tem = Path.GetFileName(fi.PathName).ToLower();
                }

                res = true;
                foreach (string s in fstr)
                {
                    if (s == "")
                        continue;

                    if (!tem.Contains(s))
                    {
                        res = false;
                        break;
                    }
                }
                if (res)
                {
                    count++;
                    res_Dir.Add(fi);
                }
            }

            foreach (Form3.FileItem fi in Form3.FilesList)
            {
                if (FindinPath)
                {
                    tem = fi.PathName.ToLower();
                }
                else
                {
                    tem = Path.GetFileName(fi.PathName).ToLower();
                }

                res = true;
                foreach (string s in fstr)
                {
                    if (s == "")
                        continue;

                    if (!tem.Contains(s))
                    {
                        res = false;
                        break;
                    }
                }
                if (res)
                {
                    count++;
                    res_File.Add(fi);
                }
            }

            myFileInfoCompare mc = new myFileInfoCompare();

            if (!checkBox2.Checked)
            {
                //目录与文件分开排序
                res_Dir.Sort(mc);
                res_File.Sort(mc);

                foreach (Form3.FileItem fi in res_Dir)
                {
                    listBox1.Items.Add(fi.PathName);
                }
                foreach (Form3.FileItem fi in res_File)
                {
                    listBox1.Items.Add(fi.PathName);
                }
            }
            else
            {
                //目录与文件混排
                res_File.AddRange(res_Dir);
                res_File.Sort(mc);
                foreach (Form3.FileItem fi in res_File)
                {
                    listBox1.Items.Add(fi.PathName);
                }
            }

            label1.Text = "共找到 " + count + " 个结果";
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel3.Text = "";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBoxTimer.Enabled = false;
            TextBoxTimer.Enabled = true;
        }

        private void TextBoxTimer_Tick(object sender, EventArgs e)
        {
            TextBoxTimer.Enabled = false;
            SearchFiles();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            try
            {
                System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo(listBox1.SelectedItem.ToString());
                System.Diagnostics.Process Pro = System.Diagnostics.Process.Start(Info);
            }
            catch
            {
            }

            if (!StrQueue.Contains(textBox1.Text.Trim()))
            {
                StrQueue.Add(textBox1.Text.Trim());
            }

            this.Hide();
        }

        private void 打开文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            string str = Path.GetDirectoryName(listBox1.SelectedItem.ToString()) ;

            try
            {
                System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo(str);
                System.Diagnostics.Process Pro = System.Diagnostics.Process.Start(Info);
            }
            catch
            {
                MessageBox.Show("打开文件或目录时发生异常！");
            }

            if (!StrQueue.Contains(textBox1.Text.Trim()))
            {
                StrQueue.Add(textBox1.Text.Trim());
            }

            this.Hide();
        }

        private bool indexState = false;
        private int IndexCount = -1;

        private void IndexStateTimer_Tick(object sender, EventArgs e)
        {
            if (Form3.StateOK)
            {
                if (Form3.FilesList.Count == 0)
                {
                    label2.Text = "没有索引记录，请先添加要监控的文件夹！";
                    IndexCount = Form3.FilesList.Count;
                    indexState = Form3.StateOK;
                }
                else if (Form3.StateOK != indexState || Form3.FilesList.Count != IndexCount)
                {
                    label2.Text = "索引完成，共 " + Form3.FilesList.Count + " 个文件。";
                    IndexCount = Form3.FilesList.Count;
                    indexState = Form3.StateOK;
                }
            }
            else
            {
                if (Form3.StateOK != indexState || Form3.FilesList.Count != IndexCount)
                {
                    label2.Text = "正在建立索引，已检索到 " + Form3.FilesList.Count + " 个文件";
                    IndexCount = Form3.FilesList.Count;
                    indexState = Form3.StateOK;
                }

            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1_DoubleClick(null, null);

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            listBox1.Width = this.Width - 40;
            listBox1.Height = this.Height - statusStrip1.Height - 120;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form2 FM = new Form2();
            FM.Show();
            this.Hide();
        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (listBox1.Items.Count > 0)
                {
                    listBox1.Focus();
                    listBox1.SelectedIndex = 0;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (StrQueue.Count > 0)
                {
                    textBox1.Text = (string)StrQueue[pStrQueue--];
                    if (pStrQueue == -1)
                        pStrQueue = StrQueue.Count - 1;
                    e.Handled = true;
                    textBox1.SelectionStart = textBox1.Text.Length;
                    textBox1.SelectionLength = 0;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (textBox1.Text != "")
                {
                    listBox1.Items.Clear();
                    textBox1.Text = "";
                    label1.Text = "";
                }
                else
                {
                    this.Hide();
                }
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && listBox1.SelectedItem != null)
            {
                listBox1_DoubleClick(null, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (textBox1.Text != "")
                {
                    listBox1.Items.Clear();
                    textBox1.Text = "";
                    label1.Text = "";
                    textBox1.Focus();
                }
                else
                {
                    this.Hide();
                }
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBox1.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    listBox1.SelectedIndex = index;
                    this.contextMenuStrip1.Show(listBox1, e.Location);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SearchFiles();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form3.InitSearch();
            SearchFiles();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.DrawBackground();

            Brush br;
            int r = e.Index % 2;
            if (r == 0)
                br = Brushes.White;
            else
                br = Brushes.MintCream;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                br = Brushes.LightSkyBlue;
                e.Graphics.FillRectangle(br, e.Bounds);
                e.DrawFocusRectangle();
            }
            else
                e.Graphics.FillRectangle(br, e.Bounds);

            Brush b = Brushes.Black;
            e.Graphics.DrawString(this.listBox1.Items[e.Index].ToString(), this.listBox1.Font, b, e.Bounds);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SearchFiles();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filepathname = listBox1.SelectedItem.ToString();
            FileInfo fi = new FileInfo(filepathname);

            toolStripStatusLabel1.Text = fi.Name;
            if (fi.Attributes != FileAttributes.Directory)
            {
                if (fi.Length < 1024)
                    toolStripStatusLabel2.Text = fi.Length.ToString() + "B";
                else if (fi.Length < 1024 * 1024)
                    toolStripStatusLabel2.Text = (fi.Length / (float)(1024)).ToString("F2") + "KB";
                else
                    toolStripStatusLabel2.Text = (fi.Length / (float)(1024 * 1024)).ToString("F2") + "MB";
            }
            else
            {
                toolStripStatusLabel2.Text = "文件夹";
            }
            toolStripStatusLabel3.Text = fi.LastWriteTime.ToString();
        }
    }
}
