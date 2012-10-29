namespace Tests
{
	partial class frmMain
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
			this.components = new System.ComponentModel.Container();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabItems = new System.Windows.Forms.TabPage();
			this.lvConsole = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabTooltipSearch = new System.Windows.Forms.TabPage();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.cmPictures = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabItem = new System.Windows.Forms.TabPage();
			this.panelDebugPictures = new System.Windows.Forms.FlowLayoutPanel();
			this.tbItemSpecs = new System.Windows.Forms.TextBox();
			this.pbItem = new System.Windows.Forms.PictureBox();
			this.sfdImages = new System.Windows.Forms.SaveFileDialog();
			this.toClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabControl1.SuspendLayout();
			this.tabItems.SuspendLayout();
			this.tabTooltipSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.cmPictures.SuspendLayout();
			this.tabItem.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbItem)).BeginInit();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabItems);
			this.tabControl1.Controls.Add(this.tabTooltipSearch);
			this.tabControl1.Controls.Add(this.tabItem);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(3, 3);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(859, 671);
			this.tabControl1.TabIndex = 2;
			// 
			// tabItems
			// 
			this.tabItems.Controls.Add(this.lvConsole);
			this.tabItems.Location = new System.Drawing.Point(4, 22);
			this.tabItems.Name = "tabItems";
			this.tabItems.Padding = new System.Windows.Forms.Padding(3);
			this.tabItems.Size = new System.Drawing.Size(851, 645);
			this.tabItems.TabIndex = 0;
			this.tabItems.Text = "Items (F7)";
			this.tabItems.UseVisualStyleBackColor = true;
			// 
			// lvConsole
			// 
			this.lvConsole.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
			this.lvConsole.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvConsole.FullRowSelect = true;
			this.lvConsole.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvConsole.Location = new System.Drawing.Point(3, 3);
			this.lvConsole.Name = "lvConsole";
			this.lvConsole.Size = new System.Drawing.Size(845, 639);
			this.lvConsole.TabIndex = 1;
			this.lvConsole.UseCompatibleStateImageBehavior = false;
			this.lvConsole.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name - mine";
			this.columnHeader1.Width = 232;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Name - theirs";
			this.columnHeader2.Width = 229;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Time - mine [ms]";
			this.columnHeader3.Width = 93;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Time - theirs [ms]";
			this.columnHeader4.Width = 97;
			// 
			// tabTooltipSearch
			// 
			this.tabTooltipSearch.Controls.Add(this.pictureBox1);
			this.tabTooltipSearch.Location = new System.Drawing.Point(4, 22);
			this.tabTooltipSearch.Name = "tabTooltipSearch";
			this.tabTooltipSearch.Padding = new System.Windows.Forms.Padding(3);
			this.tabTooltipSearch.Size = new System.Drawing.Size(851, 645);
			this.tabTooltipSearch.TabIndex = 1;
			this.tabTooltipSearch.Text = "Tooltip search (F8)";
			this.tabTooltipSearch.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.ContextMenuStrip = this.cmPictures;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(845, 639);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// cmPictures
			// 
			this.cmPictures.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.toClipboardToolStripMenuItem});
			this.cmPictures.Name = "cmPictures";
			this.cmPictures.Size = new System.Drawing.Size(153, 70);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveAsToolStripMenuItem.Text = "&Save As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// tabItem
			// 
			this.tabItem.Controls.Add(this.panelDebugPictures);
			this.tabItem.Controls.Add(this.tbItemSpecs);
			this.tabItem.Controls.Add(this.pbItem);
			this.tabItem.Location = new System.Drawing.Point(4, 22);
			this.tabItem.Name = "tabItem";
			this.tabItem.Padding = new System.Windows.Forms.Padding(3);
			this.tabItem.Size = new System.Drawing.Size(851, 645);
			this.tabItem.TabIndex = 2;
			this.tabItem.Text = "Item (F9)";
			this.tabItem.UseVisualStyleBackColor = true;
			// 
			// panelDebugPictures
			// 
			this.panelDebugPictures.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelDebugPictures.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.panelDebugPictures.Location = new System.Drawing.Point(401, 7);
			this.panelDebugPictures.Name = "panelDebugPictures";
			this.panelDebugPictures.Size = new System.Drawing.Size(447, 635);
			this.panelDebugPictures.TabIndex = 2;
			// 
			// tbItemSpecs
			// 
			this.tbItemSpecs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.tbItemSpecs.BackColor = System.Drawing.Color.White;
			this.tbItemSpecs.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tbItemSpecs.Location = new System.Drawing.Point(7, 479);
			this.tbItemSpecs.Multiline = true;
			this.tbItemSpecs.Name = "tbItemSpecs";
			this.tbItemSpecs.ReadOnly = true;
			this.tbItemSpecs.Size = new System.Drawing.Size(387, 160);
			this.tbItemSpecs.TabIndex = 1;
			// 
			// pbItem
			// 
			this.pbItem.ContextMenuStrip = this.cmPictures;
			this.pbItem.Location = new System.Drawing.Point(7, 7);
			this.pbItem.Name = "pbItem";
			this.pbItem.Size = new System.Drawing.Size(387, 465);
			this.pbItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pbItem.TabIndex = 0;
			this.pbItem.TabStop = false;
			// 
			// sfdImages
			// 
			this.sfdImages.AddExtension = false;
			this.sfdImages.Filter = "PNG Files|*.png";
			this.sfdImages.Title = "Save image as...";
			// 
			// toClipboardToolStripMenuItem
			// 
			this.toClipboardToolStripMenuItem.Name = "toClipboardToolStripMenuItem";
			this.toClipboardToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.toClipboardToolStripMenuItem.Text = "To &Clipboard";
			this.toClipboardToolStripMenuItem.Click += new System.EventHandler(this.toClipboardToolStripMenuItem_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(865, 677);
			this.Controls.Add(this.tabControl1);
			this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::Tests.Properties.Settings.Default, "WindowLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.DoubleBuffered = true;
			this.Location = global::Tests.Properties.Settings.Default.WindowLocation;
			this.Name = "frmMain";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.Text = "frmMain";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.tabControl1.ResumeLayout(false);
			this.tabItems.ResumeLayout(false);
			this.tabTooltipSearch.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.cmPictures.ResumeLayout(false);
			this.tabItem.ResumeLayout(false);
			this.tabItem.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbItem)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabItems;
		private System.Windows.Forms.ListView lvConsole;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.TabPage tabTooltipSearch;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TabPage tabItem;
		private System.Windows.Forms.TextBox tbItemSpecs;
		private System.Windows.Forms.PictureBox pbItem;
		private System.Windows.Forms.FlowLayoutPanel panelDebugPictures;
		private System.Windows.Forms.ContextMenuStrip cmPictures;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog sfdImages;
		private System.Windows.Forms.ToolStripMenuItem toClipboardToolStripMenuItem;

	}
}