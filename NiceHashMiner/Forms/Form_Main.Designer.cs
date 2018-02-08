namespace NiceHashMiner
{
    partial class Form_Main
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
            this.buttonStartMining = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusBtcPerDayLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusBtcPerDayValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusUsdPerDayLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusUsdPerDayValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelBalanceDollarValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonStopMining = new System.Windows.Forms.Button();
            this.buttonBenchmark = new System.Windows.Forms.Button();
            this.signOutButton = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.devicesListViewEnableControl1 = new NiceHashMiner.Forms.Components.DevicesListViewEnableControl();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStartMining
            // 
            this.buttonStartMining.Location = new System.Drawing.Point(12, 12);
            this.buttonStartMining.Name = "buttonStartMining";
            this.buttonStartMining.Size = new System.Drawing.Size(89, 23);
            this.buttonStartMining.TabIndex = 6;
            this.buttonStartMining.Text = "&Start";
            this.buttonStartMining.UseVisualStyleBackColor = true;
            this.buttonStartMining.Click += new System.EventHandler(this.buttonStartMining_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusBtcPerDayLabel,
            this.toolStripStatusBtcPerDayValue,
            this.toolStripStatusUsdPerDayLabel,
            this.toolStripStatusUsdPerDayValue,
            this.toolStripStatusLabelBalanceDollarValue});
            this.statusStrip.Location = new System.Drawing.Point(0, 190);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(424, 22);
            this.statusStrip.TabIndex = 8;
            // 
            // toolStripStatusBtcPerDayLabel
            // 
            this.toolStripStatusBtcPerDayLabel.Name = "toolStripStatusBtcPerDayLabel";
            this.toolStripStatusBtcPerDayLabel.Size = new System.Drawing.Size(57, 17);
            this.toolStripStatusBtcPerDayLabel.Text = "BTC/Day:";
            // 
            // toolStripStatusBtcPerDayValue
            // 
            this.toolStripStatusBtcPerDayValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusBtcPerDayValue.Name = "toolStripStatusBtcPerDayValue";
            this.toolStripStatusBtcPerDayValue.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusBtcPerDayValue.Text = "0.00000000";
            // 
            // toolStripStatusUsdPerDayLabel
            // 
            this.toolStripStatusUsdPerDayLabel.Name = "toolStripStatusUsdPerDayLabel";
            this.toolStripStatusUsdPerDayLabel.Size = new System.Drawing.Size(54, 17);
            this.toolStripStatusUsdPerDayLabel.Text = "USD/Day";
            // 
            // toolStripStatusUsdPerDayValue
            // 
            this.toolStripStatusUsdPerDayValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusUsdPerDayValue.Name = "toolStripStatusUsdPerDayValue";
            this.toolStripStatusUsdPerDayValue.Size = new System.Drawing.Size(31, 17);
            this.toolStripStatusUsdPerDayValue.Text = "0.00";
            // 
            // toolStripStatusLabelBalanceDollarValue
            // 
            this.toolStripStatusLabelBalanceDollarValue.Name = "toolStripStatusLabelBalanceDollarValue";
            this.toolStripStatusLabelBalanceDollarValue.Size = new System.Drawing.Size(16, 17);
            this.toolStripStatusLabelBalanceDollarValue.Text = "$ ";
            // 
            // buttonStopMining
            // 
            this.buttonStopMining.Enabled = false;
            this.buttonStopMining.Location = new System.Drawing.Point(107, 12);
            this.buttonStopMining.Name = "buttonStopMining";
            this.buttonStopMining.Size = new System.Drawing.Size(89, 23);
            this.buttonStopMining.TabIndex = 7;
            this.buttonStopMining.Text = "St&op";
            this.buttonStopMining.UseVisualStyleBackColor = true;
            this.buttonStopMining.Click += new System.EventHandler(this.buttonStopMining_Click);
            // 
            // buttonBenchmark
            // 
            this.buttonBenchmark.Location = new System.Drawing.Point(202, 12);
            this.buttonBenchmark.Name = "buttonBenchmark";
            this.buttonBenchmark.Size = new System.Drawing.Size(89, 23);
            this.buttonBenchmark.TabIndex = 4;
            this.buttonBenchmark.Text = "&Benchmark";
            this.buttonBenchmark.UseVisualStyleBackColor = true;
            this.buttonBenchmark.Click += new System.EventHandler(this.buttonBenchmark_Click);
            // 
            // signOutButton
            // 
            this.signOutButton.Location = new System.Drawing.Point(347, 12);
            this.signOutButton.Name = "signOutButton";
            this.signOutButton.Size = new System.Drawing.Size(75, 23);
            this.signOutButton.TabIndex = 110;
            this.signOutButton.Text = "Sign Out";
            this.signOutButton.UseVisualStyleBackColor = true;
            this.signOutButton.Click += new System.EventHandler(this.signOutButton_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // devicesListViewEnableControl1
            // 
            this.devicesListViewEnableControl1.BenchmarkCalculation = null;
            this.devicesListViewEnableControl1.FirstColumnText = "Enabled";
            this.devicesListViewEnableControl1.IsInBenchmark = false;
            this.devicesListViewEnableControl1.IsMining = false;
            this.devicesListViewEnableControl1.Location = new System.Drawing.Point(12, 41);
            this.devicesListViewEnableControl1.Name = "devicesListViewEnableControl1";
            this.devicesListViewEnableControl1.SaveToGeneralConfig = false;
            this.devicesListViewEnableControl1.Size = new System.Drawing.Size(410, 148);
            this.devicesListViewEnableControl1.TabIndex = 109;
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 212);
            this.Controls.Add(this.signOutButton);
            this.Controls.Add(this.devicesListViewEnableControl1);
            this.Controls.Add(this.buttonBenchmark);
            this.Controls.Add(this.buttonStopMining);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.buttonStartMining);
            this.Enabled = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::NiceHashMiner.Properties.Resources.logo;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(440, 250);
            this.MinimumSize = new System.Drawing.Size(440, 250);
            this.Name = "Form_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stakhanov";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form_Main_Shown);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartMining;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusBtcPerDayLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusBtcPerDayValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusUsdPerDayLabel;
        private System.Windows.Forms.Button buttonStopMining;
        private System.Windows.Forms.Button buttonBenchmark;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusUsdPerDayValue;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBalanceDollarValue;
        private Forms.Components.DevicesListViewEnableControl devicesListViewEnableControl1;
        private System.Windows.Forms.Button signOutButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
    }
}



