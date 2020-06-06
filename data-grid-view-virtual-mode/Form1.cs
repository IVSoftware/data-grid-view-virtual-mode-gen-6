using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            gridview.DragMode = (DragMode)comboBox1.SelectedIndex;

            buttonTestB4.Click += Test_DragDrop_B4;
            buttonTestFTR.Click += Test_DragDrop_FTR;

            // This is STRICTLY so that test code matches
            // the way the original post reads.
            dataList = gridview.dataList;
        }

        readonly List<DataValue> dataList;
        private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Properties.Settings.Default.ComboBoxIndex = comboBox1.SelectedIndex;
            Properties.Settings.Default.Save();
            gridview.DragMode = (DragMode)comboBox1.SelectedIndex;
            gridview.Refresh();
        }
        //void Test_DragDrop()
        //{
        //    gridview.SuspendLayout();
        //    try
        //    {
        //        //    // copy dragged row
        //        //    DataGridViewRow rowCopy = gridview.Rows[dragRow];
        //        //    DataValue dataCopy = dataList[dragRow];

        //        //    // remove dragged row
        //        //    dataList.RemoveAt(dragRow);
        //        //    gridview.Rows.RemoveAt(dragRow);

        //        //    // insert row
        //        //    dataList.Insert(row, dataCopy);
        //        //    gridview.Rows.Insert(row, rowCopy);

        //        //    // move selection to moved row
        //        //    gridview.CurrentCell = gridview[gridview.CurrentCell.ColumnIndex, row];
        //    }
        //    finally { gridview.ResumeLayout(true); }
        //}

        private void Test_DragDrop_B4(object sender, EventArgs e)
        {
            // For testing purposes:
            gridview.AllowUserToAddRows = false;

            // Added for test case "Remove Row[1] and drop it on Row[3]
            // Case 1:3 WORKS

            TestExistingCode(3, 2);
        }

        private void TestExistingCode(int dragRow, int row)
        {
            // IVS: Meaningless gesture in a VirtualMode DataGridView
            gridview.SuspendLayout();
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            try
            {
                StressTest();   // <= Added by IVS
                // OP: copy dragged row
                // IVS: This appears to use an index to assign row
                DataGridViewRow rowCopy = gridview.Rows[dragRow];

                // OP: reference to (not a copy of) the DataValue object.
                // IVS: This is using the same index to get a 
                DataValue dataCopy = dataList[dragRow];

                // OP: remove dragged row
                // IVS: This is correct
                dataList.RemoveAt(dragRow);
                StressTest();

                // IVS: This is misleading. It decrements the RowCount
                //      property which is good, but since there's no binding
                //      between the DGV and the dataList there's no meaning
                //      to removing 'at' a certain place. So this is benign.
                //      It accidentally does what you want which is to reducing
                //      the RowCount.
                gridview.Rows.RemoveAt(dragRow);
                StressTest();

                // IVS: As soon as you do this, the value of 'row' NO LONGER
                //      POINTS to where you think it does because the list has
                //      changed. (That is, it will 'work-or-not' depending on 
                //      whether it was higher or lower than dragRow before the
                //      dataList was modified by the removal.

                // OP: insert row
                // IVS: We took 'row' at the start. It NO LONGER POINTS
                //      to where you think it does, because one of the
                //      items just came out of the list and the DGV count
                //      is one less. It is INEVITABLE that this will eventually
                //      cause a problem somewhere.
                dataList.Insert(row, dataCopy); // <- DANGER: using 'row' may be invalid
                StressTest();

                // D A N G E R    Z O N E
                // This is the moment where gridview.RowCount is out of sync
                // with List.Count. If CellValueNeeded is called, it will
                // likely crash.

                // IVS: As before, there is no actual binding between dataList
                //      and Rows in the DGV. This doesn't do what you seem to 
                //      think it does.  It just increments RowCount.

                //      Also, 'row' and 'rowCopy' are unreliable variables now
                //      because changeing the list by removing one or more list
                //      items means all bets are off.
                gridview.Rows.Insert(row, rowCopy);

                // move selection to moved row
                gridview.CurrentCell = gridview[gridview.CurrentCell.ColumnIndex, row];
            }
            // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            // Added by IVS
            // Leaving out a 'catch' block often results in
            // exceptions that get swallowed and are undetectable.
            catch(Exception e)
            {
                throw e;
            }
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            finally { gridview.ResumeLayout(true); }
        }

        private void StressTest(bool @throw = true)
        {
            if (@throw) gridview.HandleError = false;
            string threatLevel = "";
            // Crash is likely if an update occurs while
            // RowCount is higher than dataList.Count
            switch (gridview.RowCount.CompareTo(dataList.Count))
            {
                case -1:
                    threatLevel = " WARNING";
                    break;
                case 0:
                    threatLevel = "";
                    break;
                case 1:
                    threatLevel = " CRASH IMMINENT";
                    break;
            }
            Debug.WriteLine("RowCount: " + gridview.RowCount + " " + "DataCount" + dataList.Count + threatLevel);

            // Force an update
            gridview.Refresh();
        }

        private void Test_DragDrop_FTR(object sender, EventArgs e)
        {
            TestIVSCode(3, 2);
        }
        private void TestIVSCode(int dragRow, int row)
        {

        }
    }
}
