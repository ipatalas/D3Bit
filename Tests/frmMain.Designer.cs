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
			this.lvConsole = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
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
			this.lvConsole.Size = new System.Drawing.Size(784, 504);
			this.lvConsole.TabIndex = 0;
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
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(790, 510);
			this.Controls.Add(this.lvConsole);
			this.DoubleBuffered = true;
			this.Name = "frmMain";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.Text = "frmMain";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView lvConsole;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
	}
}