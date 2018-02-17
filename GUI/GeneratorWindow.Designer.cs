﻿namespace GUI
{
	partial class GeneratorWindow
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.progressPanel = new System.Windows.Forms.Panel();
			this.progressGroupBox = new System.Windows.Forms.GroupBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.showPerturbedLayoutsTime = new System.Windows.Forms.NumericUpDown();
			this.showPerturbedLayouts = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.showAcceptedLayoutsTime = new System.Windows.Forms.NumericUpDown();
			this.showAcceptedLayouts = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.showFinalLayoutsTime = new System.Windows.Forms.NumericUpDown();
			this.showFinalLayouts = new System.Windows.Forms.CheckBox();
			this.infoPanel = new System.Windows.Forms.Panel();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.infoStatus = new System.Windows.Forms.Label();
			this.infoIterations = new System.Windows.Forms.Label();
			this.infoGeneratingLayout = new System.Windows.Forms.Label();
			this.infoIterationsLabel = new System.Windows.Forms.Label();
			this.infoGeneratingLayoutLabel = new System.Windows.Forms.Label();
			this.mainPictureBox = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.progressPanel.SuspendLayout();
			this.progressGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.showPerturbedLayoutsTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.showAcceptedLayoutsTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.showFinalLayoutsTime)).BeginInit();
			this.infoPanel.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.mainPictureBox, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1082, 753);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(735, 3);
			this.panel1.Name = "panel1";
			this.tableLayoutPanel1.SetRowSpan(this.panel1, 2);
			this.panel1.Size = new System.Drawing.Size(344, 747);
			this.panel1.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.progressPanel);
			this.groupBox1.Controls.Add(this.infoPanel);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(344, 747);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Controls";
			// 
			// progressPanel
			// 
			this.progressPanel.Controls.Add(this.progressGroupBox);
			this.progressPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.progressPanel.Location = new System.Drawing.Point(3, 152);
			this.progressPanel.Name = "progressPanel";
			this.progressPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.progressPanel.Size = new System.Drawing.Size(338, 199);
			this.progressPanel.TabIndex = 7;
			// 
			// progressGroupBox
			// 
			this.progressGroupBox.Controls.Add(this.label7);
			this.progressGroupBox.Controls.Add(this.label6);
			this.progressGroupBox.Controls.Add(this.showPerturbedLayoutsTime);
			this.progressGroupBox.Controls.Add(this.showPerturbedLayouts);
			this.progressGroupBox.Controls.Add(this.label2);
			this.progressGroupBox.Controls.Add(this.showAcceptedLayoutsTime);
			this.progressGroupBox.Controls.Add(this.showAcceptedLayouts);
			this.progressGroupBox.Controls.Add(this.label1);
			this.progressGroupBox.Controls.Add(this.showFinalLayoutsTime);
			this.progressGroupBox.Controls.Add(this.showFinalLayouts);
			this.progressGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressGroupBox.Location = new System.Drawing.Point(0, 0);
			this.progressGroupBox.Name = "progressGroupBox";
			this.progressGroupBox.Size = new System.Drawing.Size(338, 189);
			this.progressGroupBox.TabIndex = 0;
			this.progressGroupBox.TabStop = false;
			this.progressGroupBox.Text = "Progress showing settings";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(6, 18);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(324, 81);
			this.label7.TabIndex = 9;
			this.label7.Text = "Layout generator provides events that are fired when for example layout is accept" +
    "ed. These events can be used to show the progress of the generator.";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(300, 155);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(26, 17);
			this.label6.TabIndex = 8;
			this.label6.Text = "ms";
			// 
			// showPerturbedLayoutsTime
			// 
			this.showPerturbedLayoutsTime.Location = new System.Drawing.Point(223, 155);
			this.showPerturbedLayoutsTime.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.showPerturbedLayoutsTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.showPerturbedLayoutsTime.Name = "showPerturbedLayoutsTime";
			this.showPerturbedLayoutsTime.Size = new System.Drawing.Size(71, 22);
			this.showPerturbedLayoutsTime.TabIndex = 7;
			this.showPerturbedLayoutsTime.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			// 
			// showPerturbedLayouts
			// 
			this.showPerturbedLayouts.AutoSize = true;
			this.showPerturbedLayouts.Location = new System.Drawing.Point(6, 155);
			this.showPerturbedLayouts.Name = "showPerturbedLayouts";
			this.showPerturbedLayouts.Size = new System.Drawing.Size(197, 21);
			this.showPerturbedLayouts.TabIndex = 6;
			this.showPerturbedLayouts.Text = "Show all perturbed layouts";
			this.showPerturbedLayouts.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(300, 130);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 17);
			this.label2.TabIndex = 5;
			this.label2.Text = "ms";
			// 
			// showAcceptedLayoutsTime
			// 
			this.showAcceptedLayoutsTime.Location = new System.Drawing.Point(223, 128);
			this.showAcceptedLayoutsTime.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.showAcceptedLayoutsTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.showAcceptedLayoutsTime.Name = "showAcceptedLayoutsTime";
			this.showAcceptedLayoutsTime.Size = new System.Drawing.Size(71, 22);
			this.showAcceptedLayoutsTime.TabIndex = 4;
			this.showAcceptedLayoutsTime.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// showAcceptedLayouts
			// 
			this.showAcceptedLayouts.AutoSize = true;
			this.showAcceptedLayouts.Location = new System.Drawing.Point(6, 128);
			this.showAcceptedLayouts.Name = "showAcceptedLayouts";
			this.showAcceptedLayouts.Size = new System.Drawing.Size(175, 21);
			this.showAcceptedLayouts.TabIndex = 3;
			this.showAcceptedLayouts.Text = "Show accepted layouts";
			this.showAcceptedLayouts.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(300, 103);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "ms";
			// 
			// showFinalLayoutsTime
			// 
			this.showFinalLayoutsTime.Location = new System.Drawing.Point(223, 101);
			this.showFinalLayoutsTime.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.showFinalLayoutsTime.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.showFinalLayoutsTime.Name = "showFinalLayoutsTime";
			this.showFinalLayoutsTime.Size = new System.Drawing.Size(71, 22);
			this.showFinalLayoutsTime.TabIndex = 1;
			this.showFinalLayoutsTime.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			// 
			// showFinalLayouts
			// 
			this.showFinalLayouts.AutoSize = true;
			this.showFinalLayouts.Location = new System.Drawing.Point(6, 101);
			this.showFinalLayouts.Name = "showFinalLayouts";
			this.showFinalLayouts.Size = new System.Drawing.Size(143, 21);
			this.showFinalLayouts.TabIndex = 0;
			this.showFinalLayouts.Text = "Show final layouts";
			this.showFinalLayouts.UseVisualStyleBackColor = true;
			// 
			// infoPanel
			// 
			this.infoPanel.Controls.Add(this.groupBox2);
			this.infoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.infoPanel.Location = new System.Drawing.Point(3, 18);
			this.infoPanel.Name = "infoPanel";
			this.infoPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
			this.infoPanel.Size = new System.Drawing.Size(338, 134);
			this.infoPanel.TabIndex = 8;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.infoStatus);
			this.groupBox2.Controls.Add(this.infoIterations);
			this.groupBox2.Controls.Add(this.infoGeneratingLayout);
			this.groupBox2.Controls.Add(this.infoIterationsLabel);
			this.groupBox2.Controls.Add(this.infoGeneratingLayoutLabel);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(0, 0);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(338, 124);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Info";
			// 
			// infoStatus
			// 
			this.infoStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.infoStatus.Location = new System.Drawing.Point(6, 18);
			this.infoStatus.Name = "infoStatus";
			this.infoStatus.Size = new System.Drawing.Size(327, 32);
			this.infoStatus.TabIndex = 4;
			this.infoStatus.Text = "Status: Running";
			this.infoStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// infoIterations
			// 
			this.infoIterations.AutoSize = true;
			this.infoIterations.Location = new System.Drawing.Point(157, 94);
			this.infoIterations.Name = "infoIterations";
			this.infoIterations.Size = new System.Drawing.Size(0, 17);
			this.infoIterations.TabIndex = 3;
			// 
			// infoGeneratingLayout
			// 
			this.infoGeneratingLayout.AutoSize = true;
			this.infoGeneratingLayout.Location = new System.Drawing.Point(157, 68);
			this.infoGeneratingLayout.Name = "infoGeneratingLayout";
			this.infoGeneratingLayout.Size = new System.Drawing.Size(0, 17);
			this.infoGeneratingLayout.TabIndex = 2;
			// 
			// infoIterationsLabel
			// 
			this.infoIterationsLabel.AutoSize = true;
			this.infoIterationsLabel.Location = new System.Drawing.Point(6, 94);
			this.infoIterationsLabel.Name = "infoIterationsLabel";
			this.infoIterationsLabel.Size = new System.Drawing.Size(140, 17);
			this.infoIterationsLabel.TabIndex = 1;
			this.infoIterationsLabel.Text = "Number of iterations:";
			// 
			// infoGeneratingLayoutLabel
			// 
			this.infoGeneratingLayoutLabel.AutoSize = true;
			this.infoGeneratingLayoutLabel.Location = new System.Drawing.Point(6, 68);
			this.infoGeneratingLayoutLabel.Name = "infoGeneratingLayoutLabel";
			this.infoGeneratingLayoutLabel.Size = new System.Drawing.Size(125, 17);
			this.infoGeneratingLayoutLabel.TabIndex = 0;
			this.infoGeneratingLayoutLabel.Text = "Generating layout:";
			// 
			// mainPictureBox
			// 
			this.mainPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPictureBox.Location = new System.Drawing.Point(3, 53);
			this.mainPictureBox.Name = "mainPictureBox";
			this.mainPictureBox.Size = new System.Drawing.Size(726, 697);
			this.mainPictureBox.TabIndex = 1;
			this.mainPictureBox.TabStop = false;
			this.mainPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.mainPictureBox_Paint);
			// 
			// GeneratorWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1082, 753);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(1100, 800);
			this.Name = "GeneratorWindow";
			this.Text = "GeneratorWindow";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeneratorWindow_FormClosing);
			this.Resize += new System.EventHandler(this.GeneratorWindow_Resize);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.progressPanel.ResumeLayout(false);
			this.progressGroupBox.ResumeLayout(false);
			this.progressGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.showPerturbedLayoutsTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.showAcceptedLayoutsTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.showFinalLayoutsTime)).EndInit();
			this.infoPanel.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox mainPictureBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel progressPanel;
		private System.Windows.Forms.GroupBox progressGroupBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown showPerturbedLayoutsTime;
		private System.Windows.Forms.CheckBox showPerturbedLayouts;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown showAcceptedLayoutsTime;
		private System.Windows.Forms.CheckBox showAcceptedLayouts;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown showFinalLayoutsTime;
		private System.Windows.Forms.CheckBox showFinalLayouts;
		private System.Windows.Forms.Panel infoPanel;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label infoIterationsLabel;
		private System.Windows.Forms.Label infoGeneratingLayoutLabel;
		private System.Windows.Forms.Label infoIterations;
		private System.Windows.Forms.Label infoGeneratingLayout;
		private System.Windows.Forms.Label infoStatus;
	}
}