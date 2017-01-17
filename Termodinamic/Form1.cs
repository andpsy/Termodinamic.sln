using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace Termodinamic
{
    public partial class Form1 : Form
    {
        private static int TotalNoOfProducts = 0;
        public Dictionary<string, DataTable> dss = new Dictionary<string, DataTable>();
        public Dictionary<string, filter> Filtre = new Dictionary<string, filter>();
        public int ManufacturerScrolPos = 0;
        public int ManufacturerScrollWidth = 0;
        public int FiltersScrollPos = 0;
        public int ProductsPerLine = 4;
        public int? category_id = null, material_id = null, manufacturer_id = null;
        public string search_text = null;
        public Form1()
        {
            InitializeComponent();
            this.SizeChanged += Form1_SizeChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(CheckEnter);
            DirectoryInfo di = new DirectoryInfo("DATA");
            FileInfo[] fis = di.GetFiles();
            
            foreach (FileInfo fi in fis)
            {
                DataSet ds = new DataSet(fi.Name.Replace(fi.Extension, ""));
                ds.ReadXml(fi.FullName);
                try
                {
                    ds.Tables[0].PrimaryKey = new DataColumn[] { ds.Tables[0].Columns["ID"] };
                }
                catch { }
                ds.AcceptChanges();
                try
                {
                    dss.Add(fi.Name.Replace(fi.Extension, ""), ds.Tables[0]);
                }
                catch
                {
                    dss.Add(fi.Name.Replace(fi.Extension, ""), new DataTable(fi.Name.Replace(fi.Extension, "")));
                }
            }

            foreach (DataRow category in dss["categories"].Rows)
            {
                TreeNode tn = new TreeNode(category["NAME"].ToString());
                tn.Name = category["ID"].ToString();
                tn.NodeFont = new Font("Calibri", 10, FontStyle.Bold);
                Dictionary<int, string> uniqueMaterials = new Dictionary<int, string>();
                DataRow[] drsCategoriesProducts = dss["categories_products"].Select(String.Format("[CATEGORIE_ID]='{0}'", category["ID"].ToString()));
                foreach (DataRow drCategoryProduct in drsCategoriesProducts)
                {
                    DataRow[] drsProductsMaterials = dss["products_materials"].Select(String.Format("[PRODUCT_ID]='{0}'", drCategoryProduct["PRODUCT_ID"].ToString()));
                    foreach (DataRow drProductMaterial in drsProductsMaterials)
                    {
                        DataRow material = dss["materials"].Select(String.Format("[ID]='{0}'", drProductMaterial["MATERIAL_ID"].ToString()))[0];
                        try
                        {
                            uniqueMaterials.Add(Convert.ToInt32(material["ID"]), material["NAME"].ToString());
                            TreeNode tnChild = new TreeNode(material["NAME"].ToString());
                            tnChild.Name = material["ID"].ToString();
                            tnChild.NodeFont = new Font("Calibri", 10, FontStyle.Regular);
                            tn.Nodes.Add(tnChild);
                        }
                        catch { }
                    }
                }
                treeView1.Nodes.Add(tn);
            }

            GetManufacturers();
            GetProducts();
        }
        private void GetProducts()
        {
            //this.UseWaitCursor = true;
            //splitContainer1.Panel2.Controls.Clear();
            splitContainer3.Panel2.Controls.Clear();
            var results = from categories in dss["categories"].AsEnumerable()
                          join categories_products in dss["categories_products"].AsEnumerable() on Convert.ToInt32(categories["ID"]) equals Convert.ToInt32(categories_products["CATEGORIE_ID"])
                          join products in dss["products"].AsEnumerable() on Convert.ToInt32(categories_products["PRODUCT_ID"]) equals Convert.ToInt32(products["ID"])
                          join products_pictures in dss["products_pictures"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_pictures["PRODUCT_ID"])
                          join pictures in dss["pictures"].AsEnumerable() on Convert.ToInt32(products_pictures["PICTURE_ID"]) equals Convert.ToInt32(pictures["ID"])
                          join manufacturers_products in dss["manufacturers_products"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(manufacturers_products["PRODUCT_ID"])
                          join manufacturers in dss["manufacturers"].AsEnumerable() on Convert.ToInt32(manufacturers_products["MANUFACTURER_ID"]) equals Convert.ToInt32(manufacturers["ID"])
                          join manufacturers_pictures in dss["manufacturers_pictures"].AsEnumerable() on Convert.ToInt32(manufacturers["ID"]) equals Convert.ToInt32(manufacturers_pictures["MANUFACTURER_ID"])
                          join mpictures in dss["pictures"].AsEnumerable() on Convert.ToInt32(manufacturers_pictures["PICTURE_ID"]) equals Convert.ToInt32(mpictures["ID"])

                          join products_materials in dss["products_materials"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_materials["PRODUCT_ID"]) into pms
                          from pm in pms.DefaultIfEmpty()
                          where (Convert.ToInt32(categories["ID"]) == Convert.ToInt32(category_id) || category_id==null) && 
                            ((pm==null || Convert.ToInt32(pm["MATERIAL_ID"])==Convert.ToInt32(material_id)) || material_id == null) &&
                            (Convert.ToInt32(manufacturers["ID"]) == Convert.ToInt32(manufacturer_id) || manufacturer_id == null) &&
                            (search_text == null || (
                                products["NAME"].ToString().ToLower().Contains(search_text.ToLower()) ||
                                products["DESCRIPTION"].ToString().ToLower().Contains(search_text.ToLower()) ||
                                products["MANUFACTURER_PAGE"].ToString().ToLower().Contains(search_text.ToLower())
                                )) &&
                            Convert.ToInt32(products_pictures["DISPLAY_ORDER"]) == 1
                          select new
                          {
                              PRODUCT_ID = Convert.ToInt32(products["ID"]),
                              PRODUCT_NAME = products["NAME"].ToString(),
                              PRODUCT_PICTURE = pictures["PICTURE"].ToString(),
                              MANUFACTURER_NAME = manufacturers["NAME"].ToString(),
                              MANUFACTURER_ID = Convert.ToInt32(manufacturers["ID"]),
                              MANUFACTURER_PICTURE = mpictures["PICTURE"].ToString(),
                              FLAG = manufacturers["FLAG_LINK"].ToString(),
                              MATERIAL_ID = pm == null ? null : pm["MATERIAL_ID"].ToString()
                          };
            int i = 0;
            if (category_id == null && material_id == null && manufacturer_id == null && search_text == null) TotalNoOfProducts = results.Count();
            toolStripStatusLabel1.Text = String.Format("Inregistrari selectate: {0} din {1}.", results.Count().ToString(), TotalNoOfProducts.ToString());
            foreach (var item in results)
            {
                /*
                if (item.MATERIAL_ID != null)
                    material_id = Convert.ToInt32(item.MATERIAL_ID);
                */
                FileInfo FI = new FileInfo(Path.Combine("IMAGES", "FLAGS", String.Format("{0}.jpg", item.FLAG)));
                Image flag = Image.FromFile(FI.FullName);
                byte[] img = Convert.FromBase64String(item.MANUFACTURER_PICTURE);
                MemoryStream memStm = new MemoryStream(img);
                memStm.Seek(0, SeekOrigin.Begin);
                Image mimage = Image.FromStream(memStm);
                manufacturer m = new manufacturer(mimage, flag, item.MANUFACTURER_ID, item.MANUFACTURER_NAME);

                img = Convert.FromBase64String(item.PRODUCT_PICTURE);
                memStm = new MemoryStream(img);
                memStm.Seek(0, SeekOrigin.Begin);
                Image pimage = Image.FromStream(memStm);

                product p = new product(pimage, item.PRODUCT_NAME, m, item.PRODUCT_ID);
                p.Left = i % ProductsPerLine * (p.Width + 3) + 3;
                p.Top = (int)(i / ProductsPerLine) * (p.Height + 3) + 3;
                AttachProductEvents(p);
                //splitContainer1.Panel2.Controls.Add(p);
                splitContainer3.Panel2.Controls.Add(p);
                i++;
            }
            //this.UseWaitCursor = false;
        }

        private void AttachProductEvents(Control c)
        {
            c.Click += P_Click;
            foreach (Control child in c.Controls)
                AttachProductEvents(child);
        }

        private void P_Click(object sender, EventArgs e)
        {
            product p = null;
            try
            {
                p = ((product)sender);
            }
            catch
            {
                p = ((product)((Control)sender).Parent);
            }
            productDetail pd = GetProduct(p.product_id);
            pd.Name = "productDetail";
            splitContainer3.Visible = false;
            statusStrip1.Visible = false;
            pd.Top = 0;
            pd.Left = 0;
            pd.BringToFront();
            splitContainer1.Panel2.Controls.Add(pd);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(splitContainer1.Panel2.Controls.ContainsKey("productDetail"))
            {
                splitContainer1.Panel2.Controls["productDetail"].Dispose();
                splitContainer3.Visible = true;
                statusStrip1.Visible = true;
            }

            if (e.Node.Parent == null)
            {
                category_id = Convert.ToInt32(e.Node.Name);
                material_id = null;

                if (Filtre.Keys.Contains("material"))
                    Filtre.Remove("material");

                if (Filtre.Keys.Contains("categorie"))
                    Filtre.Remove("categorie");

                filter f = new filter("categorie", e.Node.Text, e.Node.Name);
                Filtre.Add("categorie", f);
                AddFilters();

                GetProducts();
            }
            else
            {
                material_id = Convert.ToInt32(e.Node.Name);
                category_id = Convert.ToInt32(e.Node.Parent.Name);

                if (Filtre.Keys.Contains("material"))
                    Filtre.Remove("material");
                if (Filtre.Keys.Contains("categorie"))
                    Filtre.Remove("categorie");

                filter f = new filter("categorie", e.Node.Parent.Text, e.Node.Parent.Name);
                Filtre.Add("categorie", f);
                AddFilters();
                f = new filter("material", e.Node.Text, e.Node.Name);
                Filtre.Add("material", f);
                AddFilters();

                GetProducts();
            }
        }

        public void AddFilters()
        {
            int counter = 0;
            //splitContainer3.Panel1.Controls["panelFiltre"].Controls["panelFiltreMain"].Controls.Clear();
            panelFiltreMain.Controls.Clear();
            foreach (var f in Filtre)
            {
                filter fi = (filter)f.Value;
                fi.Left = counter;
                fi.button1.Click += Button1_Click;
                //splitContainer3.Panel1.Controls["panelFiltre"].Controls["panelFiltreMain"].Controls.Add(fi);
                panelFiltreMain.Controls.Add(fi);
                counter += fi.Width + 3;
            }
            panelFiltreMain.HorizontalScroll.Maximum = counter > panelFiltreMain.Width ? counter - panelFiltreMain.Width : panelFiltreMain.Width;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            filter f = (filter)((Button)sender).Parent;
            switch (f.Tip)
            {
                case "categorie":
                    category_id = null;
                    break;
                case "material":
                    material_id = null;
                    break;
                case "producator":
                    manufacturer_id = null;
                    break;
                case "text":
                    search_text = null;
                    textBox1.Text = "";
                    break;
            }
            Filtre.Remove(f.Tip);
            AddFilters();
            GetProducts();
        }

        private int GetCurFilterPos()
        {
            int toReturn = 0;
            foreach (Control c in splitContainer3.Panel1.Controls)
                toReturn += (c.Width + 3);
            return toReturn;
        }

        private void GetManufacturers()
        {
            var results = from manufacturers in dss["manufacturers"].AsEnumerable()
                          join manufacturers_pictures in dss["manufacturers_pictures"].AsEnumerable() on Convert.ToInt32(manufacturers["ID"]) equals Convert.ToInt32(manufacturers_pictures["MANUFACTURER_ID"])
                          join pictures in dss["pictures"].AsEnumerable() on Convert.ToInt32(manufacturers_pictures["PICTURE_ID"]) equals Convert.ToInt32(pictures["ID"])
                          select new
                          {
                              MANUFACTURER_ID = Convert.ToInt32(manufacturers["ID"]),
                              MANUFACTURER_NAME = manufacturers["NAME"].ToString(),
                              MANUFACTURER_DESCRIPTION = manufacturers["DESCRIPTION"].ToString(),
                              MANUFACTURER_WEBSITE = manufacturers["WEBSITE"].ToString(),
                              FLAG = manufacturers["FLAG_LINK"].ToString(),
                              PICTURE = pictures["PICTURE"].ToString()
                          };
            int i = 0;
            int count = results.Count();
            foreach (var item in results)
            {
                FileInfo FI = new FileInfo(Path.Combine("IMAGES", "FLAGS", String.Format("{0}.jpg", item.FLAG)));
                Image flag = Image.FromFile(FI.FullName);
                byte[] img = Convert.FromBase64String(item.PICTURE);
                MemoryStream memStm = new MemoryStream(img);
                memStm.Seek(0, SeekOrigin.Begin);
                Image image = Image.FromStream(memStm);
                manufacturer m = new manufacturer(image, flag, item.MANUFACTURER_ID, item.MANUFACTURER_NAME);
                m.Left = (i < ((int)(count / 2) + count % 2)) ? i * 142 + 12 : (i - ((int)(count / 2) + count % 2)) * 142 + 12;
                m.Top = (i < ((int)(count / 2) + count % 2)) ? 0 : 37;
                AttachManufacturerEvents(m);
                splitContainer2.Panel1.Controls["panel1"].Controls.Add(m);
                i++;
            }
            ManufacturerScrollWidth = ((int)(count / 2) + count % 2) * 142 + 12 - ((Panel)splitContainer2.Panel1.Controls["panel1"]).Width;
            ((Panel)splitContainer2.Panel1.Controls["panel1"]).HorizontalScroll.Maximum = ManufacturerScrollWidth;
        }
        
        private void AttachManufacturerEvents(Control c)
        {
            c.Click += M_Click;
            foreach (Control child in c.Controls)
                AttachManufacturerEvents(child);
        }

        private void M_Click(object sender, EventArgs e)
        {
            if (splitContainer1.Panel2.Controls.ContainsKey("productDetail"))
            {
                splitContainer1.Panel2.Controls["productDetail"].Dispose();
                splitContainer3.Visible = true;
                statusStrip1.Visible = true;
            }

            manufacturer m = null;
            try
            {
                m = ((manufacturer)sender);
                manufacturer_id = m.manufacturer_id;
            }catch
            {
                m = ((manufacturer)((Control)sender).Parent);
                manufacturer_id = m.manufacturer_id;
            }

            if (Filtre.Keys.Contains("producator"))
            {
                Filtre.Remove("producator");
            }

            filter f = new filter("producator", m.manufacturer_name, m.manufacturer_id.ToString());
            Filtre.Add("producator", f);
            //f.Left = GetCurFilterPos();
            //splitContainer3.Panel1.Controls.Add(f);
            AddFilters();

            GetProducts();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ManufacturerScrolPos = ManufacturerScrolPos + 154 <= ManufacturerScrollWidth ? ManufacturerScrolPos + 154 : ManufacturerScrollWidth;
                ((Panel)splitContainer2.Panel1.Controls["panel1"]).HorizontalScroll.Value = ManufacturerScrolPos;
            }
            catch(Exception exp) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ManufacturerScrolPos = ManufacturerScrolPos - 154 <= 0 ? 0 : ManufacturerScrolPos - 154;
                ((Panel)splitContainer2.Panel1.Controls["panel1"]).HorizontalScroll.Value = ManufacturerScrolPos;
            }
            catch (Exception exp) { }
        }

        private void Form1_SizeChanged(object sender, System.EventArgs e)
        {
            ProductsPerLine = (int)(splitContainer1.Panel2.Width / 175);
            GetProducts();
        }

        private productDetail GetProduct(int _product_id)
        {
            List<string> detalii = new List<string>();
            productDetail pd = new productDetail();
            var produse = from products in dss["products"].AsEnumerable()
                          join products_datasheets in dss["products_datasheets"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_datasheets["PRODUCT_ID"])
                          join datasheets in dss["datasheets"].AsEnumerable() on Convert.ToInt32(products_datasheets["DATASHEET_ID"]) equals Convert.ToInt32(datasheets["ID"])
                          where Convert.ToInt32(products["ID"]) == Convert.ToInt32(_product_id)
                          select new
                          {
                              PRODUCT_ID = Convert.ToInt32(products["ID"]),
                              PRODUCT_NAME = products["NAME"].ToString(),
                              PRODUCT_DESCRIPTION = products["DESCRIPTION"].ToString(),
                              PRODUCT_MANUFACTURER_PAGE = products["MANUFACTURER_PAGE"].ToString(),
                              PRODUCT_DATASHEET = datasheets["DATASHEET"].ToString()
                          };
            foreach (var produs in produse)
            {
                pd = new productDetail(produs.PRODUCT_ID, produs.PRODUCT_MANUFACTURER_PAGE, produs.PRODUCT_DATASHEET);
                pd.label1.Text = produs.PRODUCT_NAME;
                string[] desc = produs.PRODUCT_DESCRIPTION.Split(';');
                //pd.textBox1.Lines = desc;
                foreach (string s in desc)
                    detalii.Add(s);
                pd.buttonExit.Click += ButtonExit_Click;
                break;
            }

            var images = from products in dss["products"].AsEnumerable() 
                          join products_pictures in dss["products_pictures"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_pictures["PRODUCT_ID"])
                          join pictures in dss["pictures"].AsEnumerable() on Convert.ToInt32(products_pictures["PICTURE_ID"]) equals Convert.ToInt32(pictures["ID"])
                          where Convert.ToInt32(products["ID"]) == Convert.ToInt32(_product_id)
                          orderby products_pictures["DISPLAY_ORDER"]
                          select new
                          {
                              PRODUCT_ID = Convert.ToInt32(products["ID"]),
                              ID_PICTURE = pictures["ID"].ToString(),
                              PRODUCT_PICTURE = pictures["PICTURE"].ToString()
                          };
            int i = 0;
            foreach (var item in images)
            {
                byte[] img = Convert.FromBase64String(item.PRODUCT_PICTURE);
                MemoryStream memStm = new MemoryStream(img);
                memStm.Seek(0, SeekOrigin.Begin);
                Image pimage = Image.FromStream(memStm);
                //pd.imageList1.Images.Add(item.ID_PICTURE, pimage);
                pd.largeImageList.Add(item.ID_PICTURE, pimage);
                i++;
            }
            pd.pictureBox1.Image = CommonFunctions.ScaleImage(pd.largeImageList.First().Value, pd.pictureBox1);
            foreach(var pair in pd.largeImageList)
            {
                Image thumb = pair.Value;
                PictureBox pb = new PictureBox();
                pb.Name = pair.Key;
                pb.Size = new Size(96, 96);
                //pb.SizeMode = thumb.Width <= pb.Width && thumb.Height <= pb.Height  ? PictureBoxSizeMode.CenterImage :  PictureBoxSizeMode.StretchImage;
                pb.SizeMode = PictureBoxSizeMode.CenterImage;
                pb.Left = pd.panel1.Controls.Count * 100;
                pb.Top = 2;
                pb.Image = CommonFunctions.ScaleImage(thumb, pb);
                pb.Click += Pb_Click;
                pd.panel1.Controls.Add(pb);
            }
            pd.panel1.HorizontalScroll.Maximum = pd.largeImageList.Count() * 100 - pd.panel1.Width;

            var conexiuni = from products in dss["products"].AsEnumerable()
                         join products_connections in dss["products_connections"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_connections["PRODUCT_ID"])
                         join connections in dss["connections"].AsEnumerable() on Convert.ToInt32(products_connections["CONNECTION_ID"]) equals Convert.ToInt32(connections["ID"])
                         where Convert.ToInt32(products["ID"]) == Convert.ToInt32(_product_id)
                         select new
                         {
                             PRODUCT_ID = Convert.ToInt32(products["ID"]),
                             ID_CONNECTION = connections["ID"].ToString(),
                             CONNECTION_NAME = connections["NAME"].ToString(),
                             CONNECTION_DESCRIPTION = connections["DESCRIPTION"].ToString()
                         };
            if (conexiuni.Count() > 0) { detalii.Add(""); detalii.Add("Conexiuni:"); }
            foreach (var item in conexiuni)
            {
                detalii.Add("- " + item.CONNECTION_NAME);
            }

            var aplicatii = from products in dss["products"].AsEnumerable()
                            join products_applications in dss["products_applications"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_applications["PRODUCT_ID"])
                            join applications in dss["applications"].AsEnumerable() on Convert.ToInt32(products_applications["APPLICATION_ID"]) equals Convert.ToInt32(applications["ID"])
                            where Convert.ToInt32(products["ID"]) == Convert.ToInt32(_product_id)
                            select new
                            {
                                PRODUCT_ID = Convert.ToInt32(products["ID"]),
                                ID_APPLICATION = applications["ID"].ToString(),
                                APPLICATION_NAME = applications["NAME"].ToString(),
                                APPLICATION_DESCRIPTION = applications["DESCRIPTION"].ToString()
                            };
            if (aplicatii.Count() > 0) { detalii.Add(""); detalii.Add("Aplicatii:"); }
            foreach (var item in aplicatii)
            {
                detalii.Add("- " + item.APPLICATION_NAME);
            }

            var actionari = from products in dss["products"].AsEnumerable()
                            join products_actuators in dss["products_actuators"].AsEnumerable() on Convert.ToInt32(products["ID"]) equals Convert.ToInt32(products_actuators["PRODUCT_ID"])
                            join actuators in dss["actuators"].AsEnumerable() on Convert.ToInt32(products_actuators["APPLICATION_ID"]) equals Convert.ToInt32(actuators["ID"])
                            where Convert.ToInt32(products["ID"]) == Convert.ToInt32(_product_id)
                            select new
                            {
                                PRODUCT_ID = Convert.ToInt32(products["ID"]),
                                ID_ACTUATOR = actuators["ID"].ToString(),
                                ACTUATOR_NAME = actuators["NAME"].ToString(),
                            };
            if (actionari.Count() > 0) { detalii.Add(""); detalii.Add("Actionari:"); }
            foreach (var item in actionari)
            {
                detalii.Add("- " + item.ACTUATOR_NAME);
            }

            pd.textBox1.Lines = detalii.ToArray();
            pd.Dock = DockStyle.Fill;
            return pd;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                FiltersScrollPos = FiltersScrollPos + 100 <= panelFiltreMain.HorizontalScroll.Maximum ? FiltersScrollPos + 100 : panelFiltreMain.HorizontalScroll.Maximum;
                panelFiltreMain.HorizontalScroll.Value = FiltersScrollPos;
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                FiltersScrollPos = FiltersScrollPos - 100 <= 1 ? 1 : FiltersScrollPos - 100;
                panelFiltreMain.HorizontalScroll.Value = FiltersScrollPos;
            }
            catch { }
        }

        private void Pb_Click(object sender, EventArgs e)
        {
            PictureBox s = (PictureBox)sender;
            productDetail parent = (productDetail)s.Parent.Parent;
            parent.pictureBox1.Image = CommonFunctions.ScaleImage(parent.largeImageList[s.Name], parent.pictureBox1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            category_id = null;
            material_id = null;
            manufacturer_id = null;
            search_text = null;
            panelFiltreMain.Controls.Clear();
            textBox1.Text = "";
            GetProducts();
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            ((Control)sender).Parent.Dispose();
            splitContainer3.Visible = true;
            statusStrip1.Visible = true;
        }

        private void CheckEnter(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                if (Filtre.Keys.Contains("text"))
                    Filtre.Remove("text");
                search_text = ((TextBox)sender).Text;
                filter f = new filter("text", search_text, search_text);
                Filtre.Add("text", f);
                AddFilters();
                GetProducts();
            }
        }
    }
}
