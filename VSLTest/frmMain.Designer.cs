namespace VSLTest
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
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnClientSendPacket = new System.Windows.Forms.Button();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.pbFileTransfer = new System.Windows.Forms.ProgressBar();
            this.btnServerSendPacket = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(12, 12);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(89, 23);
            this.btnStartServer.TabIndex = 0;
            this.btnStartServer.Text = "Server starten";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(183, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(89, 23);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Verbinden";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnClientSendPacket
            // 
            this.btnClientSendPacket.Enabled = false;
            this.btnClientSendPacket.Location = new System.Drawing.Point(183, 41);
            this.btnClientSendPacket.Name = "btnClientSendPacket";
            this.btnClientSendPacket.Size = new System.Drawing.Size(89, 23);
            this.btnClientSendPacket.TabIndex = 2;
            this.btnClientSendPacket.Text = "Senden";
            this.btnClientSendPacket.UseVisualStyleBackColor = true;
            this.btnClientSendPacket.Click += new System.EventHandler(this.btnSendPacket_Click);
            // 
            // btnSendFile
            // 
            this.btnSendFile.Enabled = false;
            this.btnSendFile.Location = new System.Drawing.Point(183, 70);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(89, 23);
            this.btnSendFile.TabIndex = 3;
            this.btnSendFile.Text = "Datei senden";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
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
            this.btnServerSendPacket.Enabled = false;
            this.btnServerSendPacket.Location = new System.Drawing.Point(13, 41);
            this.btnServerSendPacket.Name = "btnServerSendPacket";
            this.btnServerSendPacket.Size = new System.Drawing.Size(88, 23);
            this.btnServerSendPacket.TabIndex = 5;
            this.btnServerSendPacket.Text = "Senden";
            this.btnServerSendPacket.UseVisualStyleBackColor = true;
            this.btnServerSendPacket.Click += new System.EventHandler(this.btnSendPacket_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnServerSendPacket);
            this.Controls.Add(this.pbFileTransfer);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.btnClientSendPacket);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnStartServer);
            this.Name = "frmMain";
            this.Text = "VSL test application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClientSendPacket;
        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.ProgressBar pbFileTransfer;
        private System.Windows.Forms.Button btnServerSendPacket;
    }
}

