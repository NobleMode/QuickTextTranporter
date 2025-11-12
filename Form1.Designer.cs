namespace QuickTextTranporter
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tbConnectedText = new TextBox();
            label1 = new Label();
            cbDevice = new ComboBox();
            label2 = new Label();
            tbYourText = new TextBox();
            btnRefreshCbDevice = new Button();
            btnQuickClear = new Button();
            statusStrip1 = new StatusStrip();
            pbTransfer = new ToolStripProgressBar();
            tsTextStatus = new ToolStripStatusLabel();
            tsConnectedMode = new ToolStripStatusLabel();
            label3 = new Label();
            label4 = new Label();
            btnClearYourFiles = new Button();
            btnYourAddFiles = new Button();
            lvConnectedFiles = new ListView();
            lvYourFiles = new ListView();
            cbConnectedMode = new CheckBox();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tbConnectedText
            // 
            tbConnectedText.Location = new Point(12, 56);
            tbConnectedText.Multiline = true;
            tbConnectedText.Name = "tbConnectedText";
            tbConnectedText.ReadOnly = true;
            tbConnectedText.Size = new Size(252, 183);
            tbConnectedText.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 38);
            label1.Name = "label1";
            label1.Size = new Size(103, 15);
            label1.TabIndex = 1;
            label1.Text = "Connected Device";
            // 
            // cbDevice
            // 
            cbDevice.FormattingEnabled = true;
            cbDevice.Location = new Point(12, 12);
            cbDevice.Name = "cbDevice";
            cbDevice.Size = new Size(121, 23);
            cbDevice.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(280, 38);
            label2.Name = "label2";
            label2.Size = new Size(69, 15);
            label2.TabIndex = 4;
            label2.Text = "Your Device";
            // 
            // tbYourText
            // 
            tbYourText.Location = new Point(280, 56);
            tbYourText.Multiline = true;
            tbYourText.Name = "tbYourText";
            tbYourText.Size = new Size(252, 183);
            tbYourText.TabIndex = 3;
            // 
            // btnRefreshCbDevice
            // 
            btnRefreshCbDevice.Location = new Point(139, 12);
            btnRefreshCbDevice.Name = "btnRefreshCbDevice";
            btnRefreshCbDevice.Size = new Size(75, 23);
            btnRefreshCbDevice.TabIndex = 5;
            btnRefreshCbDevice.Text = "Refresh";
            btnRefreshCbDevice.UseVisualStyleBackColor = true;
            // 
            // btnQuickClear
            // 
            btnQuickClear.Location = new Point(457, 34);
            btnQuickClear.Name = "btnQuickClear";
            btnQuickClear.Size = new Size(75, 23);
            btnQuickClear.TabIndex = 6;
            btnQuickClear.Text = "Clear";
            btnQuickClear.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { pbTransfer, tsTextStatus, tsConnectedMode });
            statusStrip1.Location = new Point(0, 369);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(544, 22);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // pbTransfer
            // 
            pbTransfer.Name = "pbTransfer";
            pbTransfer.Size = new Size(100, 16);
            // 
            // tsTextStatus
            // 
            tsTextStatus.Name = "tsTextStatus";
            tsTextStatus.Size = new Size(122, 17);
            tsTextStatus.Text = "No Connected Device";
            // 
            // tsConnectedMode
            // 
            tsConnectedMode.Name = "tsConnectedMode";
            tsConnectedMode.Size = new Size(127, 17);
            tsConnectedMode.Text = "Connected Mode - Off";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 246);
            label3.Name = "label3";
            label3.Size = new Size(129, 15);
            label3.TabIndex = 8;
            label3.Text = "Connected Device Files";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(280, 246);
            label4.Name = "label4";
            label4.Size = new Size(95, 15);
            label4.TabIndex = 9;
            label4.Text = "Your Device Files";
            // 
            // btnClearYourFiles
            // 
            btnClearYourFiles.Location = new Point(457, 242);
            btnClearYourFiles.Name = "btnClearYourFiles";
            btnClearYourFiles.Size = new Size(75, 23);
            btnClearYourFiles.TabIndex = 10;
            btnClearYourFiles.Text = "Clear";
            btnClearYourFiles.UseVisualStyleBackColor = true;
            // 
            // btnYourAddFiles
            // 
            btnYourAddFiles.Location = new Point(381, 242);
            btnYourAddFiles.Name = "btnYourAddFiles";
            btnYourAddFiles.Size = new Size(75, 23);
            btnYourAddFiles.TabIndex = 11;
            btnYourAddFiles.Text = "Add";
            btnYourAddFiles.UseVisualStyleBackColor = true;
            // 
            // lvConnectedFiles
            // 
            lvConnectedFiles.Location = new Point(12, 264);
            lvConnectedFiles.MultiSelect = false;
            lvConnectedFiles.Name = "lvConnectedFiles";
            lvConnectedFiles.Size = new Size(252, 96);
            lvConnectedFiles.TabIndex = 12;
            lvConnectedFiles.UseCompatibleStateImageBehavior = false;
            // 
            // lvYourFiles
            // 
            lvYourFiles.Location = new Point(280, 264);
            lvYourFiles.Name = "lvYourFiles";
            lvYourFiles.Size = new Size(252, 96);
            lvYourFiles.TabIndex = 13;
            lvYourFiles.UseCompatibleStateImageBehavior = false;
            // 
            // cbConnectedMode
            // 
            cbConnectedMode.AutoSize = true;
            cbConnectedMode.Location = new Point(398, 12);
            cbConnectedMode.Name = "cbConnectedMode";
            cbConnectedMode.Size = new Size(134, 19);
            cbConnectedMode.TabIndex = 14;
            cbConnectedMode.Text = "Connected To Mode";
            cbConnectedMode.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(544, 391);
            Controls.Add(cbConnectedMode);
            Controls.Add(lvYourFiles);
            Controls.Add(lvConnectedFiles);
            Controls.Add(btnYourAddFiles);
            Controls.Add(btnClearYourFiles);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(statusStrip1);
            Controls.Add(btnQuickClear);
            Controls.Add(btnRefreshCbDevice);
            Controls.Add(label2);
            Controls.Add(tbYourText);
            Controls.Add(cbDevice);
            Controls.Add(label1);
            Controls.Add(tbConnectedText);
            MinimumSize = new Size(560, 430);
            Name = "Form1";
            Text = "Form1";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbConnectedText;
        private Label label1;
        private ComboBox cbDevice;
        private Label label2;
        private TextBox tbYourText;
        private Button btnRefreshCbDevice;
        private Button btnQuickClear;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsTextStatus;
        private Label label3;
        private Label label4;
        private Button btnClearYourFiles;
        private Button btnYourAddFiles;
        private ListView lvConnectedFiles;
        private ListView lvYourFiles;
        private CheckBox cbConnectedMode;
        private ToolStripStatusLabel tsConnectedMode;
        private ToolStripProgressBar pbTransfer;
    }
}
