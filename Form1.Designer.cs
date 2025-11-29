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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tbConnectedText = new TextBox();
            label1 = new Label();
            cbDevice = new ComboBox();
            label2 = new Label();
            tbYourText = new TextBox();
            btnRefreshCbDevice = new Button();
            btnQuickClear = new Button();
            statusStrip1 = new StatusStrip();
            tsTextStatus = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            tsConnectedMode = new ToolStripStatusLabel();
            toolStripStatusLabel3 = new ToolStripStatusLabel();
            ddFirewall = new ToolStripDropDownButton();
            enableFirewallRulesToolStripMenuItem = new ToolStripMenuItem();
            removeFileToolStripMenuItem = new ToolStripMenuItem();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tsWebServerCount = new ToolStripStatusLabel();
            toolStripStatusLabel5 = new ToolStripStatusLabel();
            ddWebServer = new ToolStripDropDownButton();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            copToolStripMenuItem = new ToolStripMenuItem();
            copyLocalLinkToolStripMenuItem = new ToolStripMenuItem();
            copyCloudflareLinkToolStripMenuItem = new ToolStripMenuItem();
            openWebToolStripMenuItem = new ToolStripMenuItem();
            openLocalWebToolStripMenuItem = new ToolStripMenuItem();
            openCloudflareWebToolStripMenuItem = new ToolStripMenuItem();
            label4 = new Label();
            btnClearYourFiles = new Button();
            btnYourAddFiles = new Button();
            lvConnectedFiles = new ListView();
            lvYourFiles = new ListView();
            cbConnectedMode = new CheckBox();
            pbTransfer = new ProgressBar();
            pbWebServer = new ProgressBar();
            lvWebServer = new ListView();
            label5 = new Label();
            btnRefreshWS = new Button();
            cbWebDevice = new ComboBox();
            label6 = new Label();
            tbWebServer = new TextBox();
            label3 = new Label();
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsTextStatus, toolStripStatusLabel2, tsConnectedMode, toolStripStatusLabel3, ddFirewall, toolStripStatusLabel1, tsWebServerCount, toolStripStatusLabel5, ddWebServer });
            statusStrip1.Location = new Point(0, 369);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(814, 22);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsTextStatus
            // 
            tsTextStatus.Name = "tsTextStatus";
            tsTextStatus.Size = new Size(122, 17);
            tsTextStatus.Text = "No Connected Device";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(10, 17);
            toolStripStatusLabel2.Text = "|";
            // 
            // tsConnectedMode
            // 
            tsConnectedMode.Name = "tsConnectedMode";
            tsConnectedMode.Size = new Size(127, 17);
            tsConnectedMode.Text = "Connected Mode - Off";
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new Size(10, 17);
            toolStripStatusLabel3.Text = "|";
            // 
            // ddFirewall
            // 
            ddFirewall.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddFirewall.DropDownItems.AddRange(new ToolStripItem[] { enableFirewallRulesToolStripMenuItem, removeFileToolStripMenuItem });
            ddFirewall.Image = (Image)resources.GetObject("ddFirewall.Image");
            ddFirewall.ImageTransparentColor = Color.Magenta;
            ddFirewall.Name = "ddFirewall";
            ddFirewall.Size = new Size(113, 20);
            ddFirewall.Text = "Firewall - Enabled";
            // 
            // enableFirewallRulesToolStripMenuItem
            // 
            enableFirewallRulesToolStripMenuItem.Name = "enableFirewallRulesToolStripMenuItem";
            enableFirewallRulesToolStripMenuItem.Size = new Size(191, 22);
            enableFirewallRulesToolStripMenuItem.Text = "Add Firewall Rules";
            // 
            // removeFileToolStripMenuItem
            // 
            removeFileToolStripMenuItem.Name = "removeFileToolStripMenuItem";
            removeFileToolStripMenuItem.Size = new Size(191, 22);
            removeFileToolStripMenuItem.Text = "Remove Firewall Rules";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(10, 17);
            toolStripStatusLabel1.Text = "|";
            // 
            // tsWebServerCount
            // 
            tsWebServerCount.Name = "tsWebServerCount";
            tsWebServerCount.Size = new Size(126, 17);
            tsWebServerCount.Text = "Web Server - 0 Devices";
            // 
            // toolStripStatusLabel5
            // 
            toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            toolStripStatusLabel5.Size = new Size(10, 17);
            toolStripStatusLabel5.Text = "|";
            // 
            // ddWebServer
            // 
            ddWebServer.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ddWebServer.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, copToolStripMenuItem, openWebToolStripMenuItem });
            ddWebServer.Image = (Image)resources.GetObject("ddWebServer.Image");
            ddWebServer.ImageTransparentColor = Color.Magenta;
            ddWebServer.Name = "ddWebServer";
            ddWebServer.Size = new Size(79, 20);
            ddWebServer.Text = "Web Server";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(160, 22);
            toolStripMenuItem1.Text = "Start Web Server";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(160, 22);
            toolStripMenuItem2.Text = "Stop Web Server";
            // 
            // copToolStripMenuItem
            // 
            copToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { copyLocalLinkToolStripMenuItem, copyCloudflareLinkToolStripMenuItem });
            copToolStripMenuItem.Name = "copToolStripMenuItem";
            copToolStripMenuItem.Size = new Size(160, 22);
            copToolStripMenuItem.Text = "Copy Web Link";
            // 
            // copyLocalLinkToolStripMenuItem
            // 
            copyLocalLinkToolStripMenuItem.Name = "copyLocalLinkToolStripMenuItem";
            copyLocalLinkToolStripMenuItem.Size = new Size(169, 22);
            copyLocalLinkToolStripMenuItem.Text = "Local Network";
            // 
            // copyCloudflareLinkToolStripMenuItem
            // 
            copyCloudflareLinkToolStripMenuItem.Name = "copyCloudflareLinkToolStripMenuItem";
            copyCloudflareLinkToolStripMenuItem.Size = new Size(169, 22);
            copyCloudflareLinkToolStripMenuItem.Text = "Cloudflare Tunnel";
            // 
            // openWebToolStripMenuItem
            // 
            openWebToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openLocalWebToolStripMenuItem, openCloudflareWebToolStripMenuItem });
            openWebToolStripMenuItem.Name = "openWebToolStripMenuItem";
            openWebToolStripMenuItem.Size = new Size(160, 22);
            openWebToolStripMenuItem.Text = "Open Web";
            // 
            // openLocalWebToolStripMenuItem
            // 
            openLocalWebToolStripMenuItem.Name = "openLocalWebToolStripMenuItem";
            openLocalWebToolStripMenuItem.Size = new Size(169, 22);
            openLocalWebToolStripMenuItem.Text = "Local Network";
            // 
            // openCloudflareWebToolStripMenuItem
            // 
            openCloudflareWebToolStripMenuItem.Name = "openCloudflareWebToolStripMenuItem";
            openCloudflareWebToolStripMenuItem.Size = new Size(169, 22);
            openCloudflareWebToolStripMenuItem.Text = "Cloudflare Tunnel";
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
            cbConnectedMode.Location = new Point(280, 12);
            cbConnectedMode.Name = "cbConnectedMode";
            cbConnectedMode.Size = new Size(134, 19);
            cbConnectedMode.TabIndex = 14;
            cbConnectedMode.Text = "Connected To Mode";
            cbConnectedMode.UseVisualStyleBackColor = true;
            // 
            // pbTransfer
            // 
            pbTransfer.Location = new Point(139, 242);
            pbTransfer.Name = "pbTransfer";
            pbTransfer.Size = new Size(125, 19);
            pbTransfer.TabIndex = 15;
            // 
            // pbWebServer
            // 
            pbWebServer.Location = new Point(677, 242);
            pbWebServer.Name = "pbWebServer";
            pbWebServer.Size = new Size(125, 19);
            pbWebServer.TabIndex = 22;
            // 
            // lvWebServer
            // 
            lvWebServer.Location = new Point(550, 264);
            lvWebServer.MultiSelect = false;
            lvWebServer.Name = "lvWebServer";
            lvWebServer.Size = new Size(252, 96);
            lvWebServer.TabIndex = 21;
            lvWebServer.UseCompatibleStateImageBehavior = false;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(550, 246);
            label5.Name = "label5";
            label5.Size = new Size(118, 15);
            label5.TabIndex = 20;
            label5.Text = "Connected Web Files";
            // 
            // btnRefreshWS
            // 
            btnRefreshWS.Location = new Point(677, 12);
            btnRefreshWS.Name = "btnRefreshWS";
            btnRefreshWS.Size = new Size(75, 23);
            btnRefreshWS.TabIndex = 19;
            btnRefreshWS.Text = "Refresh";
            btnRefreshWS.UseVisualStyleBackColor = true;
            // 
            // cbWebDevice
            // 
            cbWebDevice.FormattingEnabled = true;
            cbWebDevice.Location = new Point(550, 12);
            cbWebDevice.Name = "cbWebDevice";
            cbWebDevice.Size = new Size(121, 23);
            cbWebDevice.TabIndex = 18;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(550, 38);
            label6.Name = "label6";
            label6.Size = new Size(92, 15);
            label6.TabIndex = 17;
            label6.Text = "Connected Web";
            // 
            // tbWebServer
            // 
            tbWebServer.Location = new Point(550, 56);
            tbWebServer.Multiline = true;
            tbWebServer.Name = "tbWebServer";
            tbWebServer.ReadOnly = true;
            tbWebServer.Size = new Size(252, 183);
            tbWebServer.TabIndex = 23;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 246);
            label3.Name = "label3";
            label3.Size = new Size(91, 15);
            label3.TabIndex = 24;
            label3.Text = "Connected Files";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(814, 391);
            Controls.Add(label3);
            Controls.Add(tbWebServer);
            Controls.Add(pbWebServer);
            Controls.Add(lvWebServer);
            Controls.Add(label5);
            Controls.Add(btnRefreshWS);
            Controls.Add(cbWebDevice);
            Controls.Add(label6);
            Controls.Add(pbTransfer);
            Controls.Add(cbConnectedMode);
            Controls.Add(lvYourFiles);
            Controls.Add(lvConnectedFiles);
            Controls.Add(btnYourAddFiles);
            Controls.Add(btnClearYourFiles);
            Controls.Add(label4);
            Controls.Add(statusStrip1);
            Controls.Add(btnQuickClear);
            Controls.Add(btnRefreshCbDevice);
            Controls.Add(label2);
            Controls.Add(tbYourText);
            Controls.Add(cbDevice);
            Controls.Add(label1);
            Controls.Add(tbConnectedText);
            MinimumSize = new Size(830, 430);
            Name = "Form1";
            Text = "Quick Text Tranporter";
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
        private Label label4;
        private Button btnClearYourFiles;
        private Button btnYourAddFiles;
        private ListView lvConnectedFiles;
        private ListView lvYourFiles;
        private CheckBox cbConnectedMode;
        private ToolStripStatusLabel tsConnectedMode;
        private ProgressBar pbTransfer;
        private ToolStripDropDownButton ddWebServer;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripStatusLabel tsWebServerCount;
        private ToolStripStatusLabel toolStripStatusLabel5;
        private ToolStripMenuItem openWebToolStripMenuItem;
        private ToolStripMenuItem copToolStripMenuItem;
        private ListView lvWebServer;
        private ProgressBar pbWebServer;
        private Label label5;
        private Button btnRefreshWS;
        private ComboBox cbWebDevice;
        private Label label6;
        private ToolStripMenuItem removeFileToolStripMenuItem;
        private ToolStripMenuItem enableFirewallRulesToolStripMenuItem;
        private ToolStripDropDownButton ddFirewall;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private ToolStripMenuItem openLocalWebToolStripMenuItem;
        private ToolStripMenuItem openCloudflareWebToolStripMenuItem;
        private ToolStripMenuItem copyLocalLinkToolStripMenuItem;
        private ToolStripMenuItem copyCloudflareLinkToolStripMenuItem;
        private TextBox tbWebServer;
        private Label label3;
    }
}
