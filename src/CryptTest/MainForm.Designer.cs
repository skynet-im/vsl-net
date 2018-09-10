namespace CryptTest
{
    partial class MainForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.aesTab = new System.Windows.Forms.TabPage();
            this.aesControl1 = new CryptTest.AesControl();
            this.rsaTab = new System.Windows.Forms.TabPage();
            this.rsaControl1 = new CryptTest.RsaControl();
            this.e2eFileTab = new System.Windows.Forms.TabPage();
            this.e2EFileControl1 = new CryptTest.E2EFileControl();
            this.shaTab = new System.Windows.Forms.TabPage();
            this.shaControl1 = new CryptTest.ShaControl();
            this.ecdhTab = new System.Windows.Forms.TabPage();
            this.ecdhControl1 = new CryptTest.EcdhControl();
            this.tabControl1.SuspendLayout();
            this.aesTab.SuspendLayout();
            this.rsaTab.SuspendLayout();
            this.e2eFileTab.SuspendLayout();
            this.shaTab.SuspendLayout();
            this.ecdhTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.aesTab);
            this.tabControl1.Controls.Add(this.rsaTab);
            this.tabControl1.Controls.Add(this.e2eFileTab);
            this.tabControl1.Controls.Add(this.shaTab);
            this.tabControl1.Controls.Add(this.ecdhTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(410, 257);
            this.tabControl1.TabIndex = 0;
            // 
            // aesTab
            // 
            this.aesTab.Controls.Add(this.aesControl1);
            this.aesTab.Location = new System.Drawing.Point(4, 22);
            this.aesTab.Name = "aesTab";
            this.aesTab.Padding = new System.Windows.Forms.Padding(3);
            this.aesTab.Size = new System.Drawing.Size(402, 231);
            this.aesTab.TabIndex = 1;
            this.aesTab.Text = "AES";
            this.aesTab.UseVisualStyleBackColor = true;
            // 
            // aesControl1
            // 
            this.aesControl1.Location = new System.Drawing.Point(0, 0);
            this.aesControl1.Name = "aesControl1";
            this.aesControl1.Size = new System.Drawing.Size(402, 231);
            this.aesControl1.TabIndex = 0;
            // 
            // rsaTab
            // 
            this.rsaTab.Controls.Add(this.rsaControl1);
            this.rsaTab.Location = new System.Drawing.Point(4, 22);
            this.rsaTab.Name = "rsaTab";
            this.rsaTab.Size = new System.Drawing.Size(402, 231);
            this.rsaTab.TabIndex = 2;
            this.rsaTab.Text = "RSA";
            this.rsaTab.UseVisualStyleBackColor = true;
            // 
            // rsaControl1
            // 
            this.rsaControl1.Location = new System.Drawing.Point(0, 0);
            this.rsaControl1.Name = "rsaControl1";
            this.rsaControl1.Size = new System.Drawing.Size(402, 231);
            this.rsaControl1.TabIndex = 0;
            // 
            // e2eFileTab
            // 
            this.e2eFileTab.Controls.Add(this.e2EFileControl1);
            this.e2eFileTab.Location = new System.Drawing.Point(4, 22);
            this.e2eFileTab.Name = "e2eFileTab";
            this.e2eFileTab.Padding = new System.Windows.Forms.Padding(3);
            this.e2eFileTab.Size = new System.Drawing.Size(402, 231);
            this.e2eFileTab.TabIndex = 0;
            this.e2eFileTab.Text = "E2E File";
            this.e2eFileTab.UseVisualStyleBackColor = true;
            // 
            // e2EFileControl1
            // 
            this.e2EFileControl1.BackColor = System.Drawing.Color.Transparent;
            this.e2EFileControl1.Location = new System.Drawing.Point(0, 0);
            this.e2EFileControl1.Name = "e2EFileControl1";
            this.e2EFileControl1.Size = new System.Drawing.Size(402, 231);
            this.e2EFileControl1.TabIndex = 0;
            // 
            // shaTab
            // 
            this.shaTab.Controls.Add(this.shaControl1);
            this.shaTab.Location = new System.Drawing.Point(4, 22);
            this.shaTab.Name = "shaTab";
            this.shaTab.Size = new System.Drawing.Size(402, 231);
            this.shaTab.TabIndex = 3;
            this.shaTab.Text = "SHA";
            this.shaTab.UseVisualStyleBackColor = true;
            // 
            // shaControl1
            // 
            this.shaControl1.Location = new System.Drawing.Point(0, 0);
            this.shaControl1.Name = "shaControl1";
            this.shaControl1.Size = new System.Drawing.Size(402, 231);
            this.shaControl1.TabIndex = 0;
            // 
            // ecdhTab
            // 
            this.ecdhTab.Controls.Add(this.ecdhControl1);
            this.ecdhTab.Location = new System.Drawing.Point(4, 22);
            this.ecdhTab.Name = "ecdhTab";
            this.ecdhTab.Size = new System.Drawing.Size(402, 231);
            this.ecdhTab.TabIndex = 4;
            this.ecdhTab.Text = "ECDH";
            this.ecdhTab.UseVisualStyleBackColor = true;
            // 
            // ecdhControl1
            // 
            this.ecdhControl1.Location = new System.Drawing.Point(0, 0);
            this.ecdhControl1.Name = "ecdhControl1";
            this.ecdhControl1.Size = new System.Drawing.Size(402, 231);
            this.ecdhControl1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 281);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "VSL Test Anwendung für Kryptographie";
            this.tabControl1.ResumeLayout(false);
            this.aesTab.ResumeLayout(false);
            this.rsaTab.ResumeLayout(false);
            this.e2eFileTab.ResumeLayout(false);
            this.shaTab.ResumeLayout(false);
            this.ecdhTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage e2eFileTab;
        private E2EFileControl e2EFileControl1;
        private System.Windows.Forms.TabPage aesTab;
        private AesControl aesControl1;
        private System.Windows.Forms.TabPage rsaTab;
        private RsaControl rsaControl1;
        private System.Windows.Forms.TabPage shaTab;
        private ShaControl shaControl1;
        private System.Windows.Forms.TabPage ecdhTab;
        private EcdhControl ecdhControl1;
    }
}

