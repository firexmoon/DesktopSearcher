using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace 桌面搜索
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.FindStringExact(folderBrowserDialog1.SelectedPath) != -1)
                {
                }
                else
                {
                    listBox1.Items.Add(folderBrowserDialog1.SelectedPath);
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string strFilter = "";

            strFilter = " " + textBox1.Text.ToLower() + " ";
            strFilter = strFilter.Replace(',', ' ');
            strFilter = strFilter.Replace(';', ' ');
            strFilter = strFilter.Replace('|', ' ');
            strFilter = strFilter.Replace("  ", " ");
            strFilter = strFilter.Replace(" .", " *.");
            if(strFilter.Contains(" *.* "))
                strFilter = "*.*";
            strFilter = strFilter.Trim();
            if (strFilter == "")
                strFilter = "*.*";

            string str = "";
            for(int i=0;i<listBox1.Items.Count;i++)
            {
                str += listBox1.Items[i].ToString();
                str += ";";
            }

            if (str != Properties.Settings.Default.UserDirs || strFilter != Properties.Settings.Default.ExtNameFilter)
            {
                Properties.Settings.Default.ExtNameFilter = strFilter;
                Properties.Settings.Default.UserDirs = str;
                Properties.Settings.Default.Save();

                Form3.InitSearch();
            }

            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey run = hklm.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

            if (checkBox1.Checked == true)
            {
                try
                {
                    run.SetValue("桌面快捷搜索", Application.ExecutablePath);
                }
                catch (Exception my)
                {
                    MessageBox.Show(my.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    run.DeleteValue("桌面快捷搜索", false);
                }
                catch (Exception my)
                {
                    MessageBox.Show(my.Message.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            hklm.Close();

            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string str = Properties.Settings.Default.UserDirs;
            string[] dirs = str.Split(';');
            if (dirs != null)
            {
                foreach (string d in dirs)
                {
                    if (d == "")
                        continue;

                    listBox1.Items.Add(d);
                }
            }

            string Filter = Properties.Settings.Default.ExtNameFilter;
            textBox1.Text = Filter;

            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey run = hklm.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

            str = (string)run.GetValue("桌面快捷搜索");
            if (str != null)
            {
                if (str.ToLower() == Application.ExecutablePath.ToLower())
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }
            else
            {
                checkBox1.Checked = false;
            }

            hklm.Close();
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


    }
}
