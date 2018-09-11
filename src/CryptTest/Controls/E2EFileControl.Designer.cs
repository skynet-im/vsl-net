namespace CryptTest.Controls
{
    partial class E2EFileControl
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
            this.selectTargetBtn = new System.Windows.Forms.Button();
            this.targetTb = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.selectSourceBtn = new System.Windows.Forms.Button();
            this.sourceTb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.targetShaLb = new System.Windows.Forms.Label();
            this.sourceShaLb = new System.Windows.Forms.Label();
            this.decryptBtn = new System.Windows.Forms.Button();
            this.mainProgressBar = new System.Windows.Forms.ProgressBar();
            this.genIvBtn = new System.Windows.Forms.Button();
            this.ivTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.genKeyBtn = new System.Windows.Forms.Button();
            this.keyTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.encryptBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // selectTargetBtn
            // 
            this.selectTargetBtn.Location = new System.Drawing.Point(371, 39);
            this.selectTargetBtn.Name = "selectTargetBtn";
            this.selectTargetBtn.Size = new System.Drawing.Size(25, 20);
            this.selectTargetBtn.TabIndex = 11;
            this.selectTargetBtn.Text = "...";
            this.selectTargetBtn.UseVisualStyleBackColor = true;
            this.selectTargetBtn.Click += new System.EventHandler(this.selectTargetBtn_Click);
            // 
            // targetTb
            // 
            this.targetTb.Location = new System.Drawing.Point(64, 39);
            this.targetTb.Name = "targetTb";
            this.targetTb.Size = new System.Drawing.Size(301, 20);
            this.targetTb.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Zielpfad";
            // 
            // selectSourceBtn
            // 
            this.selectSourceBtn.Location = new System.Drawing.Point(371, 13);
            this.selectSourceBtn.Name = "selectSourceBtn";
            this.selectSourceBtn.Size = new System.Drawing.Size(25, 20);
            this.selectSourceBtn.TabIndex = 8;
            this.selectSourceBtn.Text = "...";
            this.selectSourceBtn.UseVisualStyleBackColor = true;
            this.selectSourceBtn.Click += new System.EventHandler(this.selectSourceBtn_Click);
            // 
            // sourceTb
            // 
            this.sourceTb.Location = new System.Drawing.Point(64, 13);
            this.sourceTb.Name = "sourceTb";
            this.sourceTb.Size = new System.Drawing.Size(301, 20);
            this.sourceTb.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Quellpfad";
            // 
            // targetShaLb
            // 
            this.targetShaLb.AutoSize = true;
            this.targetShaLb.Location = new System.Drawing.Point(6, 152);
            this.targetShaLb.Name = "targetShaLb";
            this.targetShaLb.Size = new System.Drawing.Size(93, 13);
            this.targetShaLb.TabIndex = 25;
            this.targetShaLb.Text = "Zieldatei SHA256:";
            // 
            // sourceShaLb
            // 
            this.sourceShaLb.AutoSize = true;
            this.sourceShaLb.Location = new System.Drawing.Point(6, 127);
            this.sourceShaLb.Name = "sourceShaLb";
            this.sourceShaLb.Size = new System.Drawing.Size(100, 13);
            this.sourceShaLb.TabIndex = 24;
            this.sourceShaLb.Text = "Quelldatei SHA256:";
            // 
            // decryptBtn
            // 
            this.decryptBtn.Location = new System.Drawing.Point(312, 172);
            this.decryptBtn.Name = "decryptBtn";
            this.decryptBtn.Size = new System.Drawing.Size(84, 23);
            this.decryptBtn.TabIndex = 23;
            this.decryptBtn.Text = "Entschlüsseln";
            this.decryptBtn.UseVisualStyleBackColor = true;
            this.decryptBtn.Click += new System.EventHandler(this.decryptBtn_Click);
            // 
            // mainProgressBar
            // 
            this.mainProgressBar.Location = new System.Drawing.Point(9, 201);
            this.mainProgressBar.Name = "mainProgressBar";
            this.mainProgressBar.Size = new System.Drawing.Size(387, 23);
            this.mainProgressBar.TabIndex = 22;
            // 
            // genIvBtn
            // 
            this.genIvBtn.Location = new System.Drawing.Point(321, 90);
            this.genIvBtn.Name = "genIvBtn";
            this.genIvBtn.Size = new System.Drawing.Size(75, 23);
            this.genIvBtn.TabIndex = 21;
            this.genIvBtn.Text = "Generieren";
            this.genIvBtn.UseVisualStyleBackColor = true;
            this.genIvBtn.Click += new System.EventHandler(this.genIvBtn_Click);
            // 
            // ivTb
            // 
            this.ivTb.Location = new System.Drawing.Point(64, 92);
            this.ivTb.Name = "ivTb";
            this.ivTb.Size = new System.Drawing.Size(251, 20);
            this.ivTb.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "IV";
            // 
            // genKeyBtn
            // 
            this.genKeyBtn.Location = new System.Drawing.Point(321, 64);
            this.genKeyBtn.Name = "genKeyBtn";
            this.genKeyBtn.Size = new System.Drawing.Size(75, 23);
            this.genKeyBtn.TabIndex = 18;
            this.genKeyBtn.Text = "Generieren";
            this.genKeyBtn.UseVisualStyleBackColor = true;
            this.genKeyBtn.Click += new System.EventHandler(this.genKeyBtn_Click);
            // 
            // keyTb
            // 
            this.keyTb.Location = new System.Drawing.Point(64, 66);
            this.keyTb.Name = "keyTb";
            this.keyTb.Size = new System.Drawing.Size(251, 20);
            this.keyTb.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Key";
            // 
            // encryptBtn
            // 
            this.encryptBtn.Location = new System.Drawing.Point(222, 172);
            this.encryptBtn.Name = "encryptBtn";
            this.encryptBtn.Size = new System.Drawing.Size(84, 23);
            this.encryptBtn.TabIndex = 26;
            this.encryptBtn.Text = "Verschlüsseln";
            this.encryptBtn.UseVisualStyleBackColor = true;
            this.encryptBtn.Click += new System.EventHandler(this.encryptBtn_Click);
            // 
            // E2EFileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.encryptBtn);
            this.Controls.Add(this.targetShaLb);
            this.Controls.Add(this.sourceShaLb);
            this.Controls.Add(this.decryptBtn);
            this.Controls.Add(this.mainProgressBar);
            this.Controls.Add(this.genIvBtn);
            this.Controls.Add(this.ivTb);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.genKeyBtn);
            this.Controls.Add(this.keyTb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.selectTargetBtn);
            this.Controls.Add(this.targetTb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.selectSourceBtn);
            this.Controls.Add(this.sourceTb);
            this.Controls.Add(this.label1);
            this.Name = "E2EFileControl";
            this.Size = new System.Drawing.Size(402, 231);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button selectTargetBtn;
        private System.Windows.Forms.TextBox targetTb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button selectSourceBtn;
        private System.Windows.Forms.TextBox sourceTb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label targetShaLb;
        private System.Windows.Forms.Label sourceShaLb;
        private System.Windows.Forms.Button decryptBtn;
        private System.Windows.Forms.ProgressBar mainProgressBar;
        private System.Windows.Forms.Button genIvBtn;
        private System.Windows.Forms.TextBox ivTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button genKeyBtn;
        private System.Windows.Forms.TextBox keyTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button encryptBtn;
    }
}
