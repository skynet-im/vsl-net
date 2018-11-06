namespace CryptTest.Controls
{
    partial class ShaControl
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
            this.gbShaMode = new System.Windows.Forms.GroupBox();
            this.RbEncodingHex = new System.Windows.Forms.RadioButton();
            this.RbEncodingUTF8 = new System.Windows.Forms.RadioButton();
            this.Tb2Iteration = new System.Windows.Forms.TextBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.Tb1Iteration = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.TbPlaintext = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.gbShaMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbShaMode
            // 
            this.gbShaMode.Controls.Add(this.RbEncodingHex);
            this.gbShaMode.Controls.Add(this.RbEncodingUTF8);
            this.gbShaMode.Location = new System.Drawing.Point(37, 119);
            this.gbShaMode.Name = "gbShaMode";
            this.gbShaMode.Size = new System.Drawing.Size(332, 90);
            this.gbShaMode.TabIndex = 23;
            this.gbShaMode.TabStop = false;
            this.gbShaMode.Text = "Encoding";
            // 
            // RbEncodingHex
            // 
            this.RbEncodingHex.AutoSize = true;
            this.RbEncodingHex.Location = new System.Drawing.Point(5, 42);
            this.RbEncodingHex.Name = "RbEncodingHex";
            this.RbEncodingHex.Size = new System.Drawing.Size(85, 17);
            this.RbEncodingHex.TabIndex = 1;
            this.RbEncodingHex.Text = "Hexadezimal";
            this.RbEncodingHex.UseVisualStyleBackColor = true;
            this.RbEncodingHex.CheckedChanged += new System.EventHandler(this.RbEncodingHex_CheckedChanged);
            // 
            // RbEncodingUTF8
            // 
            this.RbEncodingUTF8.AutoSize = true;
            this.RbEncodingUTF8.Checked = true;
            this.RbEncodingUTF8.Location = new System.Drawing.Point(6, 19);
            this.RbEncodingUTF8.Name = "RbEncodingUTF8";
            this.RbEncodingUTF8.Size = new System.Drawing.Size(55, 17);
            this.RbEncodingUTF8.TabIndex = 0;
            this.RbEncodingUTF8.TabStop = true;
            this.RbEncodingUTF8.Text = "UTF-8";
            this.RbEncodingUTF8.UseVisualStyleBackColor = true;
            this.RbEncodingUTF8.CheckedChanged += new System.EventHandler(this.RbEncodingUTF8_CheckedChanged);
            // 
            // Tb2Iteration
            // 
            this.Tb2Iteration.Location = new System.Drawing.Point(100, 79);
            this.Tb2Iteration.Name = "Tb2Iteration";
            this.Tb2Iteration.ReadOnly = true;
            this.Tb2Iteration.Size = new System.Drawing.Size(269, 20);
            this.Tb2Iteration.TabIndex = 22;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(34, 82);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(57, 13);
            this.Label4.TabIndex = 21;
            this.Label4.Text = "2. Iteration";
            // 
            // Tb1Iteration
            // 
            this.Tb1Iteration.Location = new System.Drawing.Point(100, 50);
            this.Tb1Iteration.Name = "Tb1Iteration";
            this.Tb1Iteration.ReadOnly = true;
            this.Tb1Iteration.Size = new System.Drawing.Size(269, 20);
            this.Tb1Iteration.TabIndex = 20;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(34, 53);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(57, 13);
            this.Label5.TabIndex = 19;
            this.Label5.Text = "1. Iteration";
            // 
            // TbPlaintext
            // 
            this.TbPlaintext.Location = new System.Drawing.Point(100, 21);
            this.TbPlaintext.Name = "TbPlaintext";
            this.TbPlaintext.Size = new System.Drawing.Size(269, 20);
            this.TbPlaintext.TabIndex = 18;
            this.TbPlaintext.UseSystemPasswordChar = true;
            this.TbPlaintext.TextChanged += new System.EventHandler(this.TbPlaintext_TextChanged);
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(34, 24);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(50, 13);
            this.Label6.TabIndex = 17;
            this.Label6.Text = "Passwort";
            // 
            // ShaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbShaMode);
            this.Controls.Add(this.Tb2Iteration);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Tb1Iteration);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.TbPlaintext);
            this.Controls.Add(this.Label6);
            this.Name = "ShaControl";
            this.Size = new System.Drawing.Size(402, 231);
            this.gbShaMode.ResumeLayout(false);
            this.gbShaMode.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.GroupBox gbShaMode;
        internal System.Windows.Forms.RadioButton RbEncodingHex;
        internal System.Windows.Forms.RadioButton RbEncodingUTF8;
        internal System.Windows.Forms.TextBox Tb2Iteration;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.TextBox Tb1Iteration;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.TextBox TbPlaintext;
        internal System.Windows.Forms.Label Label6;
    }
}
