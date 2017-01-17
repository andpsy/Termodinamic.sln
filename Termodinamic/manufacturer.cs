using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Termodinamic
{
    public partial class manufacturer : UserControl
    {
        public int manufacturer_id;
        public string manufacturer_name;
        public manufacturer()
        {
            InitializeComponent();
        }
        public manufacturer(Image image1, Image image2, int _manufacturer_id, string _manufacturer_name)
        {
            InitializeComponent();
            AttachEvents(this);
            pictureBox1.Image = image1;
            pictureBox2.Image = image2;
            manufacturer_id = _manufacturer_id;
            manufacturer_name = _manufacturer_name;
        }

        private void AttachEvents(Control parent)
        {
            parent.MouseMove += C_MouseMove;
            parent.MouseLeave += C_MouseLeave;
            foreach (Control c in parent.Controls)
            {
                AttachEvents(c);
            }
        }

        private void C_MouseLeave(object sender, EventArgs e)
        {
            this.BorderStyle = BorderStyle.None;
        }

        private void C_MouseMove(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
