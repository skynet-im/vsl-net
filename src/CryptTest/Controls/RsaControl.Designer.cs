namespace CryptTest.Controls
{
    partial class RsaControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRsaExtract = new System.Windows.Forms.Button();
            this.btnRsaGenerate = new System.Windows.Forms.Button();
            this.btnRsaEncrypt = new System.Windows.Forms.Button();
            this.btnRsaDecrypt = new System.Windows.Forms.Button();
            this.Label10 = new System.Windows.Forms.Label();
            this.tbRsaCiphertext = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.tbRsaPlaintext = new System.Windows.Forms.TextBox();
            this.tbRsaPublicKey = new System.Windows.Forms.TextBox();
            this.tbRsaPrivateKey = new System.Windows.Forms.TextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRsaExtract
            // 
            this.btnRsaExtract.Location = new System.Drawing.Point(103, 184);
            this.btnRsaExtract.Name = "btnRsaExtract";
            this.btnRsaExtract.Size = new System.Drawing.Size(75, 23);
            this.btnRsaExtract.TabIndex = 24;
            this.btnRsaExtract.Text = "extrahieren";
            this.btnRsaExtract.UseVisualStyleBackColor = true;
            this.btnRsaExtract.Click += new System.EventHandler(this.btnRsaExtract_Click);
            // 
            // btnRsaGenerate
            // 
            this.btnRsaGenerate.Location = new System.Drawing.Point(18, 184);
            this.btnRsaGenerate.Name = "btnRsaGenerate";
            this.btnRsaGenerate.Size = new System.Drawing.Size(79, 23);
            this.btnRsaGenerate.TabIndex = 23;
            this.btnRsaGenerate.Text = "generieren";
            this.btnRsaGenerate.UseVisualStyleBackColor = true;
            this.btnRsaGenerate.Click += new System.EventHandler(this.btnRsaGenerate_Click);
            // 
            // btnRsaEncrypt
            // 
            this.btnRsaEncrypt.Location = new System.Drawing.Point(184, 184);
            this.btnRsaEncrypt.Name = "btnRsaEncrypt";
            this.btnRsaEncrypt.Size = new System.Drawing.Size(92, 23);
            this.btnRsaEncrypt.TabIndex = 22;
            this.btnRsaEncrypt.Text = "verschlüsseln";
            this.btnRsaEncrypt.UseVisualStyleBackColor = true;
            this.btnRsaEncrypt.Click += new System.EventHandler(this.btnRsaEncrypt_Click);
            // 
            // btnRsaDecrypt
            // 
            this.btnRsaDecrypt.Location = new System.Drawing.Point(282, 184);
            this.btnRsaDecrypt.Name = "btnRsaDecrypt";
            this.btnRsaDecrypt.Size = new System.Drawing.Size(92, 23);
            this.btnRsaDecrypt.TabIndex = 21;
            this.btnRsaDecrypt.Text = "entschlüsseln";
            this.btnRsaDecrypt.UseVisualStyleBackColor = true;
            this.btnRsaDecrypt.Click += new System.EventHandler(this.btnRsaDecrypt_Click);
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(15, 133);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(58, 13);
            this.Label10.TabIndex = 20;
            this.Label10.Text = "CipherText";
            // 
            // tbRsaCiphertext
            // 
            this.tbRsaCiphertext.Location = new System.Drawing.Point(94, 130);
            this.tbRsaCiphertext.Multiline = true;
            this.tbRsaCiphertext.Name = "tbRsaCiphertext";
            this.tbRsaCiphertext.Size = new System.Drawing.Size(280, 48);
            this.tbRsaCiphertext.TabIndex = 19;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Location = new System.Drawing.Point(15, 79);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(51, 13);
            this.Label9.TabIndex = 18;
            this.Label9.Text = "PlainText";
            // 
            // tbRsaPlaintext
            // 
            this.tbRsaPlaintext.Location = new System.Drawing.Point(94, 76);
            this.tbRsaPlaintext.Multiline = true;
            this.tbRsaPlaintext.Name = "tbRsaPlaintext";
            this.tbRsaPlaintext.Size = new System.Drawing.Size(280, 48);
            this.tbRsaPlaintext.TabIndex = 17;
            // 
            // tbRsaPublicKey
            // 
            this.tbRsaPublicKey.Location = new System.Drawing.Point(94, 50);
            this.tbRsaPublicKey.Name = "tbRsaPublicKey";
            this.tbRsaPublicKey.Size = new System.Drawing.Size(280, 20);
            this.tbRsaPublicKey.TabIndex = 16;
            // 
            // tbRsaPrivateKey
            // 
            this.tbRsaPrivateKey.Location = new System.Drawing.Point(94, 24);
            this.tbRsaPrivateKey.Name = "tbRsaPrivateKey";
            this.tbRsaPrivateKey.Size = new System.Drawing.Size(280, 20);
            this.tbRsaPrivateKey.TabIndex = 15;
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(15, 53);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(54, 13);
            this.Label8.TabIndex = 14;
            this.Label8.Text = "PublicKey";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(15, 27);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(58, 13);
            this.Label7.TabIndex = 13;
            this.Label7.Text = "PrivateKey";
            // 
            // RsaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRsaExtract);
            this.Controls.Add(this.btnRsaGenerate);
            this.Controls.Add(this.btnRsaEncrypt);
            this.Controls.Add(this.btnRsaDecrypt);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.tbRsaCiphertext);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.tbRsaPlaintext);
            this.Controls.Add(this.tbRsaPublicKey);
            this.Controls.Add(this.tbRsaPrivateKey);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label7);
            this.Name = "RsaControl";
            this.Size = new System.Drawing.Size(402, 231);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnRsaExtract;
        internal System.Windows.Forms.Button btnRsaGenerate;
        internal System.Windows.Forms.Button btnRsaEncrypt;
        internal System.Windows.Forms.Button btnRsaDecrypt;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.TextBox tbRsaCiphertext;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.TextBox tbRsaPlaintext;
        internal System.Windows.Forms.TextBox tbRsaPublicKey;
        internal System.Windows.Forms.TextBox tbRsaPrivateKey;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label7;
    }
}
