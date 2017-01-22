using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Termodinamic
{
    public partial class Loader : Form
    {
        public static int timer = 0;
        public Loader()
        {
            InitializeComponent();
            this.Shown += Loader_Shown;
        }

        private void Loader_Load(object sender, EventArgs e)
        {
            //timer1.Start();
        }

        private void Loader_Shown(object sender, System.EventArgs e)
        {
            this.Refresh();
            if (!Get45or451FromRegistry())
            {
                DialogResult ans = MessageBox.Show("Acest program necesita \"Microsoft .NET Framework 4.5.2\" pentru a rula. Doriti instalarea acestuia?", "", MessageBoxButtons.YesNo);
                if (ans == DialogResult.Yes)
                {
                    string path = Path.Combine(Environment.CurrentDirectory, "dotnetfx452", "NDP452-KB2901907-x86-x64-AllOS-ENU.exe");
                    Process p = new Process();
                    p.EnableRaisingEvents = true;
                    p = Process.Start(path);
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        LaunchProgram();
                    }else
                    {
                        MessageBox.Show("A aparut o eroare la instalarea \"Microsoft .NET Framework 4.5.2\"! Aplicatia se va inchide.");
                        Application.Exit();
                    }
                }else
                {
                    Application.Exit();
                }
            }
            else
            {
                LaunchProgram();
            }
        }

        private void LaunchProgram()
        {
            Form1 frm = new Form1();
            frm.Visible = false;
            //frm.Form1_Load(this, null);
            frm.ShowDialog(this);
            //this.Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (timer)
            {
                case 0:
                    panel1.Visible = true;
                    timer++;
                    break;
                case 1:
                    panel2.Visible = true;
                    timer++;
                    break;
                case 2:
                    panel3.Visible = true;
                    timer++;
                    break;
                case 3:
                    panel4.Visible = true;
                    timer++;
                    break;
                case 4:
                    panel5.Visible = true;
                    timer++;
                    break;
                case 5:
                    panel1.Visible = panel2.Visible = panel3.Visible = panel4.Visible = panel5.Visible = false;
                    timer = 0;
                    break;
            }
        }

        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        private static bool Get45or451FromRegistry()
        {
            try
            {
                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        if ((int)ndpKey.GetValue("Release") >= 379893) return true;
                    }
                }
                return false;
            }catch(Exception exp) { return false; }
        }
    }
}
