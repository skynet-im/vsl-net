namespace CryptTest.Controls
{
    partial class AesControl
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
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.EncodingHexRb = new System.Windows.Forms.RadioButton();
            this.EncodingUTF8Rb = new System.Windows.Forms.RadioButton();
            this.btnAesGenerateIV = new System.Windows.Forms.Button();
            this.tbAesIV = new System.Windows.Forms.TextBox();
            this.Label14 = new System.Windows.Forms.Label();
            this.btnAesEncrypt = new System.Windows.Forms.Button();
            this.btnAesDecrypt = new System.Windows.Forms.Button();
            this.tbAesCipherText = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.tbAesPlainText = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.btnAesGenerate = new System.Windows.Forms.Button();
            this.tbAesKey = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.EncodingHexRb);
            this.GroupBox1.Controls.Add(this.EncodingUTF8Rb);
            this.GroupBox1.Location = new System.Drawing.Point(25, 138);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(200, 68);
            this.GroupBox1.TabIndex = 26;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Kodierung";
            // 
            // EncodingHexRb
            // 
            this.EncodingHexRb.AutoSize = true;
            this.EncodingHexRb.Location = new System.Drawing.Point(6, 42);
            this.EncodingHexRb.Name = "EncodingHexRb";
            this.EncodingHexRb.Size = new System.Drawing.Size(85, 17);
            this.EncodingHexRb.TabIndex = 13;
            this.EncodingHexRb.Text = "Hexadezimal";
            this.EncodingHexRb.UseVisualStyleBackColor = true;
            // 
            // EncodingUTF8Rb
            // 
            this.EncodingUTF8Rb.AutoSize = true;
            this.EncodingUTF8Rb.Checked = true;
            this.EncodingUTF8Rb.Location = new System.Drawing.Point(6, 19);
            this.EncodingUTF8Rb.Name = "EncodingUTF8Rb";
            this.EncodingUTF8Rb.Size = new System.Drawing.Size(55, 17);
            this.EncodingUTF8Rb.TabIndex = 12;
            this.EncodingUTF8Rb.TabStop = true;
            this.EncodingUTF8Rb.Text = "UTF-8";
            this.EncodingUTF8Rb.UseVisualStyleBackColor = true;
            this.EncodingUTF8Rb.CheckedChanged += new System.EventHandler(this.RadioButtons_CheckedChanged);
            // 
            // btnAesGenerateIV
            // 
            this.btnAesGenerateIV.Location = new System.Drawing.Point(303, 49);
            this.btnAesGenerateIV.Name = "btnAesGenerateIV";
            this.btnAesGenerateIV.Size = new System.Drawing.Size(86, 23);
            this.btnAesGenerateIV.TabIndex = 25;
            this.btnAesGenerateIV.Text = "generieren";
            this.btnAesGenerateIV.UseVisualStyleBackColor = true;
            this.btnAesGenerateIV.Click += new System.EventHandler(this.btnAesGenerateIV_Click);
            // 
            // tbAesIV
            // 
            this.tbAesIV.Location = new System.Drawing.Point(78, 51);
            this.tbAesIV.Name = "tbAesIV";
            this.tbAesIV.Size = new System.Drawing.Size(219, 20);
            this.tbAesIV.TabIndex = 24;
            // 
            // Label14
            // 
            this.Label14.AutoSize = true;
            this.Label14.Location = new System.Drawing.Point(14, 54);
            this.Label14.Name = "Label14";
            this.Label14.Size = new System.Drawing.Size(17, 13);
            this.Label14.TabIndex = 23;
            this.Label14.Text = "IV";
            // 
            // btnAesEncrypt
            // 
            this.btnAesEncrypt.Location = new System.Drawing.Point(303, 101);
            this.btnAesEncrypt.Name = "btnAesEncrypt";
            this.btnAesEncrypt.Size = new System.Drawing.Size(86, 23);
            this.btnAesEncrypt.TabIndex = 22;
            this.btnAesEncrypt.Text = "verschlüsseln";
            this.btnAesEncrypt.UseVisualStyleBackColor = true;
            this.btnAesEncrypt.Click += new System.EventHandler(this.btnAesEncrypt_Click);
            // 
            // btnAesDecrypt
            // 
            this.btnAesDecrypt.Location = new System.Drawing.Point(303, 75);
            this.btnAesDecrypt.Name = "btnAesDecrypt";
            this.btnAesDecrypt.Size = new System.Drawing.Size(86, 23);
            this.btnAesDecrypt.TabIndex = 21;
            this.btnAesDecrypt.Text = "entschlüsseln";
            this.btnAesDecrypt.UseVisualStyleBackColor = true;
            this.btnAesDecrypt.Click += new System.EventHandler(this.btnAesDecrypt_Click);
            // 
            // tbAesCipherText
            // 
            this.tbAesCipherText.Location = new System.Drawing.Point(78, 103);
            this.tbAesCipherText.Name = "tbAesCipherText";
            this.tbAesCipherText.Size = new System.Drawing.Size(219, 20);
            this.tbAesCipherText.TabIndex = 20;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(14, 106);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(58, 13);
            this.Label3.TabIndex = 19;
            this.Label3.Text = "CipherText";
            // 
            // tbAesPlainText
            // 
            this.tbAesPlainText.Location = new System.Drawing.Point(78, 77);
            this.tbAesPlainText.Name = "tbAesPlainText";
            this.tbAesPlainText.Size = new System.Drawing.Size(219, 20);
            this.tbAesPlainText.TabIndex = 18;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(14, 80);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(51, 13);
            this.Label2.TabIndex = 17;
            this.Label2.Text = "PlainText";
            // 
            // btnAesGenerate
            // 
            this.btnAesGenerate.Location = new System.Drawing.Point(303, 23);
            this.btnAesGenerate.Name = "btnAesGenerate";
            this.btnAesGenerate.Size = new System.Drawing.Size(86, 23);
            this.btnAesGenerate.TabIndex = 16;
            this.btnAesGenerate.Text = "generieren";
            this.btnAesGenerate.UseVisualStyleBackColor = true;
            this.btnAesGenerate.Click += new System.EventHandler(this.btnAesGenerate_Click);
            // 
            // tbAesKey
            // 
            this.tbAesKey.Location = new System.Drawing.Point(78, 25);
            this.tbAesKey.Name = "tbAesKey";
            this.tbAesKey.Size = new System.Drawing.Size(219, 20);
            this.tbAesKey.TabIndex = 15;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(14, 28);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(25, 13);
            this.Label1.TabIndex = 14;
            this.Label1.Text = "Key";
            // 
            // AesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.btnAesGenerateIV);
            this.Controls.Add(this.tbAesIV);
            this.Controls.Add(this.Label14);
            this.Controls.Add(this.btnAesEncrypt);
            this.Controls.Add(this.btnAesDecrypt);
            this.Controls.Add(this.tbAesCipherText);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.tbAesPlainText);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.btnAesGenerate);
            this.Controls.Add(this.tbAesKey);
            this.Controls.Add(this.Label1);
            this.Name = "AesControl";
            this.Size = new System.Drawing.Size(402, 231);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.RadioButton EncodingHexRb;
        internal System.Windows.Forms.RadioButton EncodingUTF8Rb;
        internal System.Windows.Forms.Button btnAesGenerateIV;
        internal System.Windows.Forms.TextBox tbAesIV;
        internal System.Windows.Forms.Label Label14;
        internal System.Windows.Forms.Button btnAesEncrypt;
        internal System.Windows.Forms.Button btnAesDecrypt;
        internal System.Windows.Forms.TextBox tbAesCipherText;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.TextBox tbAesPlainText;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Button btnAesGenerate;
        internal System.Windows.Forms.TextBox tbAesKey;
        internal System.Windows.Forms.Label Label1;
    }
}
