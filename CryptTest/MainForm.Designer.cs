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
            this.e2eFileTab = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.e2eFile_sourceTb = new System.Windows.Forms.TextBox();
            this.e2eFile_selectSourceBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.e2eFile_selectTargetBtn = new System.Windows.Forms.Button();
            this.e2eFile_targetTb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.e2eFile_keyTb = new System.Windows.Forms.TextBox();
            this.e2eFile_genKeyBtn = new System.Windows.Forms.Button();
            this.e2eFile_genIvBtn = new System.Windows.Forms.Button();
            this.e2eFile_ivTb = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.e2eFilePb = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.e2eFile_sourceShaLb = new System.Windows.Forms.Label();
            this.e2eFile_targetShaLb = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.e2eFileTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.e2eFileTab);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(410, 257);
            this.tabControl1.TabIndex = 0;
            // 
            // e2eFileTab
            // 
            this.e2eFileTab.Controls.Add(this.e2eFile_targetShaLb);
            this.e2eFileTab.Controls.Add(this.e2eFile_sourceShaLb);
            this.e2eFileTab.Controls.Add(this.button1);
            this.e2eFileTab.Controls.Add(this.e2eFilePb);
            this.e2eFileTab.Controls.Add(this.e2eFile_genIvBtn);
            this.e2eFileTab.Controls.Add(this.e2eFile_ivTb);
            this.e2eFileTab.Controls.Add(this.label4);
            this.e2eFileTab.Controls.Add(this.e2eFile_genKeyBtn);
            this.e2eFileTab.Controls.Add(this.e2eFile_keyTb);
            this.e2eFileTab.Controls.Add(this.label3);
            this.e2eFileTab.Controls.Add(this.e2eFile_selectTargetBtn);
            this.e2eFileTab.Controls.Add(this.e2eFile_targetTb);
            this.e2eFileTab.Controls.Add(this.label2);
            this.e2eFileTab.Controls.Add(this.e2eFile_selectSourceBtn);
            this.e2eFileTab.Controls.Add(this.e2eFile_sourceTb);
            this.e2eFileTab.Controls.Add(this.label1);
            this.e2eFileTab.Location = new System.Drawing.Point(4, 22);
            this.e2eFileTab.Name = "e2eFileTab";
            this.e2eFileTab.Padding = new System.Windows.Forms.Padding(3);
            this.e2eFileTab.Size = new System.Drawing.Size(402, 231);
            this.e2eFileTab.TabIndex = 0;
            this.e2eFileTab.Text = "E2E File";
            this.e2eFileTab.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Quellpfad";
            // 
            // e2eFile_sourceTb
            // 
            this.e2eFile_sourceTb.Location = new System.Drawing.Point(64, 13);
            this.e2eFile_sourceTb.Name = "e2eFile_sourceTb";
            this.e2eFile_sourceTb.Size = new System.Drawing.Size(301, 20);
            this.e2eFile_sourceTb.TabIndex = 1;
            // 
            // e2eFile_selectSourceBtn
            // 
            this.e2eFile_selectSourceBtn.Location = new System.Drawing.Point(371, 13);
            this.e2eFile_selectSourceBtn.Name = "e2eFile_selectSourceBtn";
            this.e2eFile_selectSourceBtn.Size = new System.Drawing.Size(25, 20);
            this.e2eFile_selectSourceBtn.TabIndex = 2;
            this.e2eFile_selectSourceBtn.Text = "...";
            this.e2eFile_selectSourceBtn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Zielpfad";
            // 
            // e2eFile_selectTargetBtn
            // 
            this.e2eFile_selectTargetBtn.Location = new System.Drawing.Point(371, 39);
            this.e2eFile_selectTargetBtn.Name = "e2eFile_selectTargetBtn";
            this.e2eFile_selectTargetBtn.Size = new System.Drawing.Size(25, 20);
            this.e2eFile_selectTargetBtn.TabIndex = 5;
            this.e2eFile_selectTargetBtn.Text = "...";
            this.e2eFile_selectTargetBtn.UseVisualStyleBackColor = true;
            // 
            // e2eFile_targetTb
            // 
            this.e2eFile_targetTb.Location = new System.Drawing.Point(64, 39);
            this.e2eFile_targetTb.Name = "e2eFile_targetTb";
            this.e2eFile_targetTb.Size = new System.Drawing.Size(301, 20);
            this.e2eFile_targetTb.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Key";
            // 
            // e2eFile_keyTb
            // 
            this.e2eFile_keyTb.Location = new System.Drawing.Point(64, 67);
            this.e2eFile_keyTb.Name = "e2eFile_keyTb";
            this.e2eFile_keyTb.Size = new System.Drawing.Size(251, 20);
            this.e2eFile_keyTb.TabIndex = 7;
            // 
            // e2eFile_genKeyBtn
            // 
            this.e2eFile_genKeyBtn.Location = new System.Drawing.Point(321, 65);
            this.e2eFile_genKeyBtn.Name = "e2eFile_genKeyBtn";
            this.e2eFile_genKeyBtn.Size = new System.Drawing.Size(75, 23);
            this.e2eFile_genKeyBtn.TabIndex = 8;
            this.e2eFile_genKeyBtn.Text = "Generieren";
            this.e2eFile_genKeyBtn.UseVisualStyleBackColor = true;
            // 
            // e2eFile_genIvBtn
            // 
            this.e2eFile_genIvBtn.Location = new System.Drawing.Point(321, 91);
            this.e2eFile_genIvBtn.Name = "e2eFile_genIvBtn";
            this.e2eFile_genIvBtn.Size = new System.Drawing.Size(75, 23);
            this.e2eFile_genIvBtn.TabIndex = 11;
            this.e2eFile_genIvBtn.Text = "Generieren";
            this.e2eFile_genIvBtn.UseVisualStyleBackColor = true;
            // 
            // e2eFile_ivTb
            // 
            this.e2eFile_ivTb.Location = new System.Drawing.Point(64, 93);
            this.e2eFile_ivTb.Name = "e2eFile_ivTb";
            this.e2eFile_ivTb.Size = new System.Drawing.Size(251, 20);
            this.e2eFile_ivTb.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "IV";
            // 
            // e2eFilePb
            // 
            this.e2eFilePb.Location = new System.Drawing.Point(9, 202);
            this.e2eFilePb.Name = "e2eFilePb";
            this.e2eFilePb.Size = new System.Drawing.Size(387, 23);
            this.e2eFilePb.TabIndex = 12;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(321, 173);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // e2eFile_sourceShaLb
            // 
            this.e2eFile_sourceShaLb.AutoSize = true;
            this.e2eFile_sourceShaLb.Location = new System.Drawing.Point(6, 128);
            this.e2eFile_sourceShaLb.Name = "e2eFile_sourceShaLb";
            this.e2eFile_sourceShaLb.Size = new System.Drawing.Size(100, 13);
            this.e2eFile_sourceShaLb.TabIndex = 14;
            this.e2eFile_sourceShaLb.Text = "Quelldatei SHA256:";
            // 
            // e2eFile_targetShaLb
            // 
            this.e2eFile_targetShaLb.AutoSize = true;
            this.e2eFile_targetShaLb.Location = new System.Drawing.Point(6, 153);
            this.e2eFile_targetShaLb.Name = "e2eFile_targetShaLb";
            this.e2eFile_targetShaLb.Size = new System.Drawing.Size(93, 13);
            this.e2eFile_targetShaLb.TabIndex = 15;
            this.e2eFile_targetShaLb.Text = "Zieldatei SHA256:";
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
            this.Text = "VSL Test Application for Cryptography";
            this.tabControl1.ResumeLayout(false);
            this.e2eFileTab.ResumeLayout(false);
            this.e2eFileTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage e2eFileTab;
        private System.Windows.Forms.Button e2eFile_selectTargetBtn;
        private System.Windows.Forms.TextBox e2eFile_targetTb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button e2eFile_selectSourceBtn;
        private System.Windows.Forms.TextBox e2eFile_sourceTb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button e2eFile_genIvBtn;
        private System.Windows.Forms.TextBox e2eFile_ivTb;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button e2eFile_genKeyBtn;
        private System.Windows.Forms.TextBox e2eFile_keyTb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label e2eFile_targetShaLb;
        private System.Windows.Forms.Label e2eFile_sourceShaLb;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar e2eFilePb;
    }
}

