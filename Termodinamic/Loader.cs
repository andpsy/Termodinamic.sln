using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
