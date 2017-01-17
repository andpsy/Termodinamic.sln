using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Termodinamic
{
    public partial class productDetail : UserControl
    {
        public int ThumbSrollPos = 0;
        public int product_id;
        public string manufacturer_page;
        public string data_sheet;
        public Dictionary<string, Image> largeImageList = new Dictionary<string, Image>();
        public productDetail()
        {
            InitializeComponent();
        }

        public productDetail(int _product_id, string _manufacturer_page, string _data_sheet)
        {
            InitializeComponent();
            product_id = _product_id;
            manufacturer_page = _manufacturer_page;
            data_sheet = _data_sheet;
        }

        private void buttonImgRight_Click(object sender, EventArgs e)
        {
            try
            {
                ThumbSrollPos = ThumbSrollPos + 96 <= panel1.HorizontalScroll.Maximum ? ThumbSrollPos + 96 : panel1.HorizontalScroll.Maximum;
                panel1.HorizontalScroll.Value = ThumbSrollPos;
            }
            catch (Exception exp) { }
        }

        private void buttonImgLeft_Click(object sender, EventArgs e)
        {
            try
            {
                ThumbSrollPos = ThumbSrollPos - 96 <= 1 ? 1 : ThumbSrollPos - 96;
                panel1.HorizontalScroll.Value = ThumbSrollPos;
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine("Pdf", data_sheet));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start(manufacturer_page);
        }
    }
}
