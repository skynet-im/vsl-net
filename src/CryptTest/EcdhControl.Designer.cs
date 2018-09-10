namespace CryptTest
{
    partial class EcdhControl
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
            this.TbBobKey = new System.Windows.Forms.TextBox();
            this.Label16 = new System.Windows.Forms.Label();
            this.TbBobPublic = new System.Windows.Forms.TextBox();
            this.Label15 = new System.Windows.Forms.Label();
            this.TbAlicePublic = new System.Windows.Forms.TextBox();
            this.BtnCreateKey = new System.Windows.Forms.Button();
            this.TbAliceKey = new System.Windows.Forms.TextBox();
            this.Label13 = new System.Windows.Forms.Label();
            this.BtnBobGenParams = new System.Windows.Forms.Button();
            this.BtnAliceGenParams = new System.Windows.Forms.Button();
            this.Label12 = new System.Windows.Forms.Label();
            this.TbBobPrivate = new System.Windows.Forms.TextBox();
            this.Label11 = new System.Windows.Forms.Label();
            this.TbAlicePrivate = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TbBobKey
            // 
            this.TbBobKey.Location = new System.Drawing.Point(76, 188);
            this.TbBobKey.Name = "TbBobKey";
            this.TbBobKey.Size = new System.Drawing.Size(191, 20);
            this.TbBobKey.TabIndex = 27;
            // 
            // Label16
            // 
            this.Label16.AutoSize = true;
            this.Label16.Location = new System.Drawing.Point(24, 126);
            this.Label16.Name = "Label16";
            this.Label16.Size = new System.Drawing.Size(36, 13);
            this.Label16.TabIndex = 26;
            this.Label16.Text = "Public";
            // 
            // TbBobPublic
            // 
            this.TbBobPublic.Location = new System.Drawing.Point(76, 123);
            this.TbBobPublic.Name = "TbBobPublic";
            this.TbBobPublic.Size = new System.Drawing.Size(191, 20);
            this.TbBobPublic.TabIndex = 25;
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Location = new System.Drawing.Point(24, 57);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(36, 13);
            this.Label15.TabIndex = 24;
            this.Label15.Text = "Public";
            // 
            // TbAlicePublic
            // 
            this.TbAlicePublic.Location = new System.Drawing.Point(76, 54);
            this.TbAlicePublic.Name = "TbAlicePublic";
            this.TbAlicePublic.Size = new System.Drawing.Size(191, 20);
            this.TbAlicePublic.TabIndex = 23;
            // 
            // BtnCreateKey
            // 
            this.BtnCreateKey.Location = new System.Drawing.Point(273, 160);
            this.BtnCreateKey.Name = "BtnCreateKey";
            this.BtnCreateKey.Size = new System.Drawing.Size(106, 23);
            this.BtnCreateKey.TabIndex = 22;
            this.BtnCreateKey.Text = "Create Key";
            this.BtnCreateKey.UseVisualStyleBackColor = true;
            this.BtnCreateKey.Click += new System.EventHandler(this.BtnCreateKey_Click);
            // 
            // TbAliceKey
            // 
            this.TbAliceKey.Location = new System.Drawing.Point(76, 162);
            this.TbAliceKey.Name = "TbAliceKey";
            this.TbAliceKey.Size = new System.Drawing.Size(191, 20);
            this.TbAliceKey.TabIndex = 21;
            // 
            // Label13
            // 
            this.Label13.AutoSize = true;
            this.Label13.Location = new System.Drawing.Point(24, 165);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(25, 13);
            this.Label13.TabIndex = 20;
            this.Label13.Text = "Key";
            // 
            // BtnBobGenParams
            // 
            this.BtnBobGenParams.Location = new System.Drawing.Point(273, 92);
            this.BtnBobGenParams.Name = "BtnBobGenParams";
            this.BtnBobGenParams.Size = new System.Drawing.Size(106, 23);
            this.BtnBobGenParams.TabIndex = 19;
            this.BtnBobGenParams.Text = "Generate Params";
            this.BtnBobGenParams.UseVisualStyleBackColor = true;
            this.BtnBobGenParams.Click += new System.EventHandler(this.BtnBobGenParams_Click);
            // 
            // BtnAliceGenParams
            // 
            this.BtnAliceGenParams.Location = new System.Drawing.Point(273, 23);
            this.BtnAliceGenParams.Name = "BtnAliceGenParams";
            this.BtnAliceGenParams.Size = new System.Drawing.Size(106, 23);
            this.BtnAliceGenParams.TabIndex = 18;
            this.BtnAliceGenParams.Text = "Generate Params";
            this.BtnAliceGenParams.UseVisualStyleBackColor = true;
            this.BtnAliceGenParams.Click += new System.EventHandler(this.BtnAliceGenParams_Click);
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Location = new System.Drawing.Point(24, 97);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(26, 13);
            this.Label12.TabIndex = 17;
            this.Label12.Text = "Bob";
            // 
            // TbBobPrivate
            // 
            this.TbBobPrivate.Location = new System.Drawing.Point(76, 94);
            this.TbBobPrivate.Name = "TbBobPrivate";
            this.TbBobPrivate.Size = new System.Drawing.Size(191, 20);
            this.TbBobPrivate.TabIndex = 16;
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Location = new System.Drawing.Point(24, 28);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(30, 13);
            this.Label11.TabIndex = 15;
            this.Label11.Text = "Alice";
            // 
            // TbAlicePrivate
            // 
            this.TbAlicePrivate.Location = new System.Drawing.Point(76, 25);
            this.TbAlicePrivate.Name = "TbAlicePrivate";
            this.TbAlicePrivate.Size = new System.Drawing.Size(191, 20);
            this.TbAlicePrivate.TabIndex = 14;
            // 
            // EcdhControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TbBobKey);
            this.Controls.Add(this.Label16);
            this.Controls.Add(this.TbBobPublic);
            this.Controls.Add(this.Label15);
            this.Controls.Add(this.TbAlicePublic);
            this.Controls.Add(this.BtnCreateKey);
            this.Controls.Add(this.TbAliceKey);
            this.Controls.Add(this.Label13);
            this.Controls.Add(this.BtnBobGenParams);
            this.Controls.Add(this.BtnAliceGenParams);
            this.Controls.Add(this.Label12);
            this.Controls.Add(this.TbBobPrivate);
            this.Controls.Add(this.Label11);
            this.Controls.Add(this.TbAlicePrivate);
            this.Name = "EcdhControl";
            this.Size = new System.Drawing.Size(402, 231);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox TbBobKey;
        internal System.Windows.Forms.Label Label16;
        internal System.Windows.Forms.TextBox TbBobPublic;
        internal System.Windows.Forms.Label Label15;
        internal System.Windows.Forms.TextBox TbAlicePublic;
        internal System.Windows.Forms.Button BtnCreateKey;
        internal System.Windows.Forms.TextBox TbAliceKey;
        internal System.Windows.Forms.Label Label13;
        internal System.Windows.Forms.Button BtnBobGenParams;
        internal System.Windows.Forms.Button BtnAliceGenParams;
        internal System.Windows.Forms.Label Label12;
        internal System.Windows.Forms.TextBox TbBobPrivate;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.TextBox TbAlicePrivate;
    }
}
