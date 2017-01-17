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
    public partial class product : UserControl
    {
        public int product_id;
        public product()
        {
            InitializeComponent();
        }

        public product(Image img, string product_name, manufacturer m, int _product_id)
        {
            InitializeComponent();
            AttachEvents(this);
            pictureBox1.Image = CommonFunctions.ScaleImage( img, this.pictureBox1);
            label1.Text = product_name;
            manufacturer1.pictureBox1.Image = m.pictureBox1.Image;
            manufacturer1.pictureBox2.Image = m.pictureBox2.Image;
            product_id = _product_id;

            //pictureBox1.SizeMode = img.Width <= pictureBox1.Width && img.Height <= pictureBox1.Height ? PictureBoxSizeMode.CenterImage : PictureBoxSizeMode.StretchImage;

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

        private void Product_MouseLeave(object sender, EventArgs e)
        {
            this.BorderStyle = BorderStyle.None;
        }

        private void Product_MouseMove(object sender, MouseEventArgs e)
        {
            this.BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
