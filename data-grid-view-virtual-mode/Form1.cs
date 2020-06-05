using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace data_grid_view_virtual_mode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = Properties.Settings.Default.ComboBoxIndex;
            comboBox1.SelectionChangeCommitted += ComboBox1_SelectionChangeCommitted;
            dataGridView1.DragMode = (DragMode)comboBox1.SelectedIndex;
        }

        private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Properties.Settings.Default.ComboBoxIndex = comboBox1.SelectedIndex;
            Properties.Settings.Default.Save();
            dataGridView1.DragMode = (DragMode)comboBox1.SelectedIndex;
            dataGridView1.Refresh();
        }
    }
}
