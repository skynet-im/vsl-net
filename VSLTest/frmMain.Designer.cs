namespace VSLTest
{
    partial class FrmMain
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
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnClientSendPacket = new System.Windows.Forms.Button();
            this.btnReceiveFile = new System.Windows.Forms.Button();
            this.pbFileTransfer = new System.Windows.Forms.ProgressBar();
            this.btnServerSendPacket = new System.Windows.Forms.Button();
            this.btnPenetrationTest = new System.Windows.Forms.Button();
            this.LbServer = new System.Windows.Forms.Label();
            this.LbServerUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.BtnCleanup = new System.Windows.Forms.Button();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(12, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(97, 23);
            this.btnStartServer.TabIndex = 0;
            this.btnStartServer.Text = "Server starten";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.BtnStartServer_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(173, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(99, 23);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Verbinden";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // btnClientSendPacket
            // 
            this.btnClientSendPacket.Enabled = false;
            this.btnClientSendPacket.Location = new System.Drawing.Point(173, 41);
            this.btnClientSendPacket.Name = "btnClientSendPacket";
            this.btnClientSendPacket.Size = new System.Drawing.Size(99, 23);
            this.btnClientSendPacket.TabIndex = 2;
            this.btnClientSendPacket.Text = "Senden";
            this.btnClientSendPacket.UseVisualStyleBackColor = true;
            this.btnClientSendPacket.Click += new System.EventHandler(this.BtnSendPacket_Click);
            // 
            // btnReceiveFile
            // 
            this.btnReceiveFile.Enabled = false;
            this.btnReceiveFile.Location = new System.Drawing.Point(173, 70);
            this.btnReceiveFile.Name = "btnReceiveFile";
            this.btnReceiveFile.Size = new System.Drawing.Size(99, 23);
            this.btnReceiveFile.TabIndex = 3;
            this.btnReceiveFile.Text = "Datei empfangen";
            this.btnReceiveFile.UseVisualStyleBackColor = true;
            this.btnReceiveFile.Click += new System.EventHandler(this.BtnReceiveFile_Click);
            // 
            // pbFileTransfer
            // 
            this.pbFileTransfer.Location = new System.Drawing.Point(12, 226);
            this.pbFileTransfer.Name = "pbFileTransfer";
            this.pbFileTransfer.Size = new System.Drawing.Size(260, 23);
            this.pbFileTransfer.TabIndex = 4;
            // 
            // btnServerSendPacket
            // 
            this.btnServerSendPacket.Location = new System.Drawing.Point(13, 41);
            this.btnServerSendPacket.Name = "btnServerSendPacket";
            this.btnServerSendPacket.Size = new System.Drawing.Size(96, 23);
            this.btnServerSendPacket.TabIndex = 5;
            this.btnServerSendPacket.Text = "Senden";
            this.btnServerSendPacket.UseVisualStyleBackColor = true;
            this.btnServerSendPacket.Click += new System.EventHandler(this.BtnSendPacket_Click);
            // 
            // btnPenetrationTest
            // 
            this.btnPenetrationTest.Location = new System.Drawing.Point(173, 128);
            this.btnPenetrationTest.Name = "btnPenetrationTest";
            this.btnPenetrationTest.Size = new System.Drawing.Size(99, 23);
            this.btnPenetrationTest.TabIndex = 6;
            this.btnPenetrationTest.Text = "Stresstest";
            this.btnPenetrationTest.UseVisualStyleBackColor = true;
            this.btnPenetrationTest.Click += new System.EventHandler(this.BtnPenetrationTest_Click);
            // 
            // LbServer
            // 
            this.LbServer.AutoSize = true;
            this.LbServer.Location = new System.Drawing.Point(13, 71);
            this.LbServer.Name = "LbServer";
            this.LbServer.Size = new System.Drawing.Size(54, 13);
            this.LbServer.TabIndex = 7;
            this.LbServer.Text = "Loading...";
            // 
            // LbServerUpdateTimer
            // 
            this.LbServerUpdateTimer.Enabled = true;
            this.LbServerUpdateTimer.Tick += new System.EventHandler(this.LbServerUpdateTimer_Tick);
            // 
            // BtnCleanup
            // 
            this.BtnCleanup.Location = new System.Drawing.Point(12, 197);
            this.BtnCleanup.Name = "BtnCleanup";
            this.BtnCleanup.Size = new System.Drawing.Size(97, 23);
            this.BtnCleanup.TabIndex = 8;
            this.BtnCleanup.Text = "Liste aufräumen";
            this.BtnCleanup.UseVisualStyleBackColor = true;
            this.BtnCleanup.Click += new System.EventHandler(this.BtnCleanup_Click);
            // 
            // btnSendFile
            // 
            this.btnSendFile.Enabled = false;
            this.btnSendFile.Location = new System.Drawing.Point(173, 99);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(99, 23);
            this.btnSendFile.TabIndex = 9;
            this.btnSendFile.Text = "Datei senden";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.BtnSendFile_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.BtnCleanup);
            this.Controls.Add(this.LbServer);
            this.Controls.Add(this.btnPenetrationTest);
            this.Controls.Add(this.btnServerSendPacket);
            this.Controls.Add(this.pbFileTransfer);
            this.Controls.Add(this.btnReceiveFile);
            this.Controls.Add(this.btnClientSendPacket);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnStartServer);
            this.Name = "FrmMain";
            this.Text = "VSL {0} Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClientSendPacket;
        private System.Windows.Forms.Button btnReceiveFile;
        private System.Windows.Forms.ProgressBar pbFileTransfer;
        private System.Windows.Forms.Button btnServerSendPacket;
        private System.Windows.Forms.Button btnPenetrationTest;
        private System.Windows.Forms.Label LbServer;
        private System.Windows.Forms.Timer LbServerUpdateTimer;
        private System.Windows.Forms.Button BtnCleanup;
        private System.Windows.Forms.Button btnSendFile;
    }
}

