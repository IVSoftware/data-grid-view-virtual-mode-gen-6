namespace data_grid_view_virtual_mode
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gridview = new data_grid_view_virtual_mode.DataGridViewVirtual();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonTestB4 = new System.Windows.Forms.Button();
            this.buttonTestFTR = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridview)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.gridview, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonTestB4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBox1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonTestFTR, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 284);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // gridview
            // 
            this.gridview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.gridview, 3);
            this.gridview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridview.DragMode = data_grid_view_virtual_mode.DragMode.Remove;
            this.gridview.Location = new System.Drawing.Point(3, 3);
            this.gridview.Name = "gridview";
            this.gridview.Size = new System.Drawing.Size(478, 251);
            this.gridview.TabIndex = 0;
            this.gridview.VirtualMode = true;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Remove Drag Items",
            "Dim Drag Items"});
            this.comboBox1.Location = new System.Drawing.Point(245, 260);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(236, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // buttonTestB4
            // 
            this.buttonTestB4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestB4.Location = new System.Drawing.Point(3, 260);
            this.buttonTestB4.Name = "buttonTestB4";
            this.buttonTestB4.Size = new System.Drawing.Size(115, 21);
            this.buttonTestB4.TabIndex = 2;
            this.buttonTestB4.Text = "Run B4 Code";
            this.buttonTestB4.UseVisualStyleBackColor = true;
            // 
            // buttonTestFTR
            // 
            this.buttonTestFTR.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestFTR.Location = new System.Drawing.Point(124, 260);
            this.buttonTestFTR.Name = "buttonTestFTR";
            this.buttonTestFTR.Size = new System.Drawing.Size(115, 21);
            this.buttonTestFTR.TabIndex = 2;
            this.buttonTestFTR.Text = "Run FTR Code";
            this.buttonTestFTR.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 284);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridViewVirtual gridview;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button buttonTestB4;
        private System.Windows.Forms.Button buttonTestFTR;
    }
}

