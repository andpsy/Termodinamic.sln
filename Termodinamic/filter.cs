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
    public partial class filter : UserControl
    {
        public string Tip;
        public string Filtru;
        public string IdFiltru;

        public filter()
        {
            InitializeComponent();
        }

        public filter(string _tip, string _filtru, string _id_filtru)
        {
            InitializeComponent();
            Tip = _tip;
            Filtru = _filtru;
            IdFiltru = _id_filtru;
            label1.Text = Tip + ": " + Filtru;
            //button1.Left = label1.Width + 3;
            //this.Width = label1.Width + button1.Width + 9;
            //button1.BringToFront();
            //this.Refresh();
        }
    }
}
