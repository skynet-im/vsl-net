<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.mainTabControl = New System.Windows.Forms.TabControl()
        Me.tpRSA = New System.Windows.Forms.TabPage()
        Me.btnRsaExtract = New System.Windows.Forms.Button()
        Me.btnRsaGenerate = New System.Windows.Forms.Button()
        Me.btnRsaEncrypt = New System.Windows.Forms.Button()
        Me.btnRsaDecrypt = New System.Windows.Forms.Button()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.tbRsaCiphertext = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.tbRsaPlaintext = New System.Windows.Forms.TextBox()
        Me.tbRsaPublicKey = New System.Windows.Forms.TextBox()
        Me.tbRsaPrivateKey = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.tpSHA = New System.Windows.Forms.TabPage()
        Me.gbShaMode = New System.Windows.Forms.GroupBox()
        Me.rbShaModeHex = New System.Windows.Forms.RadioButton()
        Me.rbShaModeUTF8 = New System.Windows.Forms.RadioButton()
        Me.tbShaHashHash = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.tbShaHash = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.tbShaPlainText = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.tpECDH = New System.Windows.Forms.TabPage()
        Me.tbECDHBobKey = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.tbECDHBobPub = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.tbECDHAlicePub = New System.Windows.Forms.TextBox()
        Me.btnECDHCreateKey = New System.Windows.Forms.Button()
        Me.tbECDHAliceKey = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.btnECDHBobGenParams = New System.Windows.Forms.Button()
        Me.btnECDHAliceGenParams = New System.Windows.Forms.Button()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.tbECDHBob = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.tbECDHAlice = New System.Windows.Forms.TextBox()
        Me.tpTest = New System.Windows.Forms.TabPage()
        Me.pbTest = New System.Windows.Forms.ProgressBar()
        Me.btnTestStart = New System.Windows.Forms.Button()
        Me.mainTabControl.SuspendLayout()
        Me.tpRSA.SuspendLayout()
        Me.tpSHA.SuspendLayout()
        Me.gbShaMode.SuspendLayout()
        Me.tpECDH.SuspendLayout()
        Me.tpTest.SuspendLayout()
        Me.SuspendLayout()
        '
        'mainTabControl
        '
        Me.mainTabControl.Controls.Add(Me.tpRSA)
        Me.mainTabControl.Controls.Add(Me.tpSHA)
        Me.mainTabControl.Controls.Add(Me.tpECDH)
        Me.mainTabControl.Controls.Add(Me.tpTest)
        Me.mainTabControl.Location = New System.Drawing.Point(12, 12)
        Me.mainTabControl.Name = "mainTabControl"
        Me.mainTabControl.SelectedIndex = 0
        Me.mainTabControl.Size = New System.Drawing.Size(378, 237)
        Me.mainTabControl.TabIndex = 0
        '
        'tpRSA
        '
        Me.tpRSA.Controls.Add(Me.btnRsaExtract)
        Me.tpRSA.Controls.Add(Me.btnRsaGenerate)
        Me.tpRSA.Controls.Add(Me.btnRsaEncrypt)
        Me.tpRSA.Controls.Add(Me.btnRsaDecrypt)
        Me.tpRSA.Controls.Add(Me.Label10)
        Me.tpRSA.Controls.Add(Me.tbRsaCiphertext)
        Me.tpRSA.Controls.Add(Me.Label9)
        Me.tpRSA.Controls.Add(Me.tbRsaPlaintext)
        Me.tpRSA.Controls.Add(Me.tbRsaPublicKey)
        Me.tpRSA.Controls.Add(Me.tbRsaPrivateKey)
        Me.tpRSA.Controls.Add(Me.Label8)
        Me.tpRSA.Controls.Add(Me.Label7)
        Me.tpRSA.Location = New System.Drawing.Point(4, 22)
        Me.tpRSA.Name = "tpRSA"
        Me.tpRSA.Padding = New System.Windows.Forms.Padding(3)
        Me.tpRSA.Size = New System.Drawing.Size(370, 211)
        Me.tpRSA.TabIndex = 0
        Me.tpRSA.Text = "RSA"
        Me.tpRSA.UseVisualStyleBackColor = True
        '
        'btnRsaExtract
        '
        Me.btnRsaExtract.Location = New System.Drawing.Point(7, 177)
        Me.btnRsaExtract.Name = "btnRsaExtract"
        Me.btnRsaExtract.Size = New System.Drawing.Size(75, 23)
        Me.btnRsaExtract.TabIndex = 12
        Me.btnRsaExtract.Text = "extrahieren"
        Me.btnRsaExtract.UseVisualStyleBackColor = True
        '
        'btnRsaGenerate
        '
        Me.btnRsaGenerate.Location = New System.Drawing.Point(88, 177)
        Me.btnRsaGenerate.Name = "btnRsaGenerate"
        Me.btnRsaGenerate.Size = New System.Drawing.Size(69, 23)
        Me.btnRsaGenerate.TabIndex = 11
        Me.btnRsaGenerate.Text = "generieren"
        Me.btnRsaGenerate.UseVisualStyleBackColor = True
        '
        'btnRsaEncrypt
        '
        Me.btnRsaEncrypt.Location = New System.Drawing.Point(163, 177)
        Me.btnRsaEncrypt.Name = "btnRsaEncrypt"
        Me.btnRsaEncrypt.Size = New System.Drawing.Size(92, 23)
        Me.btnRsaEncrypt.TabIndex = 10
        Me.btnRsaEncrypt.Text = "verschlüsseln"
        Me.btnRsaEncrypt.UseVisualStyleBackColor = True
        '
        'btnRsaDecrypt
        '
        Me.btnRsaDecrypt.Location = New System.Drawing.Point(261, 177)
        Me.btnRsaDecrypt.Name = "btnRsaDecrypt"
        Me.btnRsaDecrypt.Size = New System.Drawing.Size(92, 23)
        Me.btnRsaDecrypt.TabIndex = 9
        Me.btnRsaDecrypt.Text = "entschlüsseln"
        Me.btnRsaDecrypt.UseVisualStyleBackColor = True
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(20, 126)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(58, 13)
        Me.Label10.TabIndex = 8
        Me.Label10.Text = "CipherText"
        '
        'tbRsaCiphertext
        '
        Me.tbRsaCiphertext.Location = New System.Drawing.Point(114, 123)
        Me.tbRsaCiphertext.Multiline = True
        Me.tbRsaCiphertext.Name = "tbRsaCiphertext"
        Me.tbRsaCiphertext.Size = New System.Drawing.Size(239, 48)
        Me.tbRsaCiphertext.TabIndex = 7
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(20, 72)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(51, 13)
        Me.Label9.TabIndex = 6
        Me.Label9.Text = "PlainText"
        '
        'tbRsaPlaintext
        '
        Me.tbRsaPlaintext.Location = New System.Drawing.Point(114, 69)
        Me.tbRsaPlaintext.Multiline = True
        Me.tbRsaPlaintext.Name = "tbRsaPlaintext"
        Me.tbRsaPlaintext.Size = New System.Drawing.Size(239, 48)
        Me.tbRsaPlaintext.TabIndex = 5
        '
        'tbRsaPublicKey
        '
        Me.tbRsaPublicKey.Location = New System.Drawing.Point(114, 43)
        Me.tbRsaPublicKey.Name = "tbRsaPublicKey"
        Me.tbRsaPublicKey.Size = New System.Drawing.Size(239, 20)
        Me.tbRsaPublicKey.TabIndex = 4
        '
        'tbRsaPrivateKey
        '
        Me.tbRsaPrivateKey.Location = New System.Drawing.Point(114, 17)
        Me.tbRsaPrivateKey.Name = "tbRsaPrivateKey"
        Me.tbRsaPrivateKey.Size = New System.Drawing.Size(239, 20)
        Me.tbRsaPrivateKey.TabIndex = 3
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(20, 46)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(54, 13)
        Me.Label8.TabIndex = 2
        Me.Label8.Text = "PublicKey"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(20, 20)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(58, 13)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "PrivateKey"
        '
        'tpSHA
        '
        Me.tpSHA.Controls.Add(Me.gbShaMode)
        Me.tpSHA.Controls.Add(Me.tbShaHashHash)
        Me.tpSHA.Controls.Add(Me.Label4)
        Me.tpSHA.Controls.Add(Me.tbShaHash)
        Me.tpSHA.Controls.Add(Me.Label5)
        Me.tpSHA.Controls.Add(Me.tbShaPlainText)
        Me.tpSHA.Controls.Add(Me.Label6)
        Me.tpSHA.Location = New System.Drawing.Point(4, 22)
        Me.tpSHA.Name = "tpSHA"
        Me.tpSHA.Padding = New System.Windows.Forms.Padding(3)
        Me.tpSHA.Size = New System.Drawing.Size(370, 211)
        Me.tpSHA.TabIndex = 2
        Me.tpSHA.Text = "SHA"
        Me.tpSHA.UseVisualStyleBackColor = True
        '
        'gbShaMode
        '
        Me.gbShaMode.Controls.Add(Me.rbShaModeHex)
        Me.gbShaMode.Controls.Add(Me.rbShaModeUTF8)
        Me.gbShaMode.Location = New System.Drawing.Point(23, 101)
        Me.gbShaMode.Name = "gbShaMode"
        Me.gbShaMode.Size = New System.Drawing.Size(332, 104)
        Me.gbShaMode.TabIndex = 16
        Me.gbShaMode.TabStop = False
        Me.gbShaMode.Text = "Hashmodus"
        '
        'rbShaModeHex
        '
        Me.rbShaModeHex.AutoSize = True
        Me.rbShaModeHex.Location = New System.Drawing.Point(6, 43)
        Me.rbShaModeHex.Name = "rbShaModeHex"
        Me.rbShaModeHex.Size = New System.Drawing.Size(165, 17)
        Me.rbShaModeHex.TabIndex = 1
        Me.rbShaModeHex.Text = "Hexadezimal (Skynet-Update)"
        Me.rbShaModeHex.UseVisualStyleBackColor = True
        '
        'rbShaModeUTF8
        '
        Me.rbShaModeUTF8.AutoSize = True
        Me.rbShaModeUTF8.Checked = True
        Me.rbShaModeUTF8.Location = New System.Drawing.Point(7, 20)
        Me.rbShaModeUTF8.Name = "rbShaModeUTF8"
        Me.rbShaModeUTF8.Size = New System.Drawing.Size(172, 17)
        Me.rbShaModeUTF8.TabIndex = 0
        Me.rbShaModeUTF8.TabStop = True
        Me.rbShaModeUTF8.Text = "Standart (Skynet-Registrierung)"
        Me.rbShaModeUTF8.UseVisualStyleBackColor = True
        '
        'tbShaHashHash
        '
        Me.tbShaHashHash.Location = New System.Drawing.Point(86, 75)
        Me.tbShaHashHash.Name = "tbShaHashHash"
        Me.tbShaHashHash.ReadOnly = True
        Me.tbShaHashHash.Size = New System.Drawing.Size(269, 20)
        Me.tbShaHashHash.TabIndex = 15
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(20, 78)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(58, 13)
        Me.Label4.TabIndex = 14
        Me.Label4.Text = "CipherText"
        '
        'tbShaHash
        '
        Me.tbShaHash.Location = New System.Drawing.Point(86, 46)
        Me.tbShaHash.Name = "tbShaHash"
        Me.tbShaHash.ReadOnly = True
        Me.tbShaHash.Size = New System.Drawing.Size(269, 20)
        Me.tbShaHash.TabIndex = 13
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(20, 49)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(51, 13)
        Me.Label5.TabIndex = 12
        Me.Label5.Text = "PlainText"
        '
        'tbShaPlainText
        '
        Me.tbShaPlainText.Location = New System.Drawing.Point(86, 17)
        Me.tbShaPlainText.Name = "tbShaPlainText"
        Me.tbShaPlainText.Size = New System.Drawing.Size(269, 20)
        Me.tbShaPlainText.TabIndex = 10
        Me.tbShaPlainText.UseSystemPasswordChar = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(20, 20)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(50, 13)
        Me.Label6.TabIndex = 9
        Me.Label6.Text = "Passwort"
        '
        'tpECDH
        '
        Me.tpECDH.Controls.Add(Me.tbECDHBobKey)
        Me.tpECDH.Controls.Add(Me.Label16)
        Me.tpECDH.Controls.Add(Me.tbECDHBobPub)
        Me.tpECDH.Controls.Add(Me.Label15)
        Me.tpECDH.Controls.Add(Me.tbECDHAlicePub)
        Me.tpECDH.Controls.Add(Me.btnECDHCreateKey)
        Me.tpECDH.Controls.Add(Me.tbECDHAliceKey)
        Me.tpECDH.Controls.Add(Me.Label13)
        Me.tpECDH.Controls.Add(Me.btnECDHBobGenParams)
        Me.tpECDH.Controls.Add(Me.btnECDHAliceGenParams)
        Me.tpECDH.Controls.Add(Me.Label12)
        Me.tpECDH.Controls.Add(Me.tbECDHBob)
        Me.tpECDH.Controls.Add(Me.Label11)
        Me.tpECDH.Controls.Add(Me.tbECDHAlice)
        Me.tpECDH.Location = New System.Drawing.Point(4, 22)
        Me.tpECDH.Name = "tpECDH"
        Me.tpECDH.Padding = New System.Windows.Forms.Padding(3)
        Me.tpECDH.Size = New System.Drawing.Size(370, 211)
        Me.tpECDH.TabIndex = 4
        Me.tpECDH.Text = "ECDH"
        Me.tpECDH.UseVisualStyleBackColor = True
        '
        'tbECDHBobKey
        '
        Me.tbECDHBobKey.Location = New System.Drawing.Point(69, 177)
        Me.tbECDHBobKey.Name = "tbECDHBobKey"
        Me.tbECDHBobKey.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHBobKey.TabIndex = 13
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(18, 115)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(36, 13)
        Me.Label16.TabIndex = 12
        Me.Label16.Text = "Public"
        '
        'tbECDHBobPub
        '
        Me.tbECDHBobPub.Location = New System.Drawing.Point(69, 112)
        Me.tbECDHBobPub.Name = "tbECDHBobPub"
        Me.tbECDHBobPub.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHBobPub.TabIndex = 11
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(18, 46)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(36, 13)
        Me.Label15.TabIndex = 10
        Me.Label15.Text = "Public"
        '
        'tbECDHAlicePub
        '
        Me.tbECDHAlicePub.Location = New System.Drawing.Point(69, 43)
        Me.tbECDHAlicePub.Name = "tbECDHAlicePub"
        Me.tbECDHAlicePub.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHAlicePub.TabIndex = 9
        '
        'btnECDHCreateKey
        '
        Me.btnECDHCreateKey.Location = New System.Drawing.Point(175, 149)
        Me.btnECDHCreateKey.Name = "btnECDHCreateKey"
        Me.btnECDHCreateKey.Size = New System.Drawing.Size(106, 23)
        Me.btnECDHCreateKey.TabIndex = 8
        Me.btnECDHCreateKey.Text = "Create Key"
        Me.btnECDHCreateKey.UseVisualStyleBackColor = True
        '
        'tbECDHAliceKey
        '
        Me.tbECDHAliceKey.Location = New System.Drawing.Point(69, 151)
        Me.tbECDHAliceKey.Name = "tbECDHAliceKey"
        Me.tbECDHAliceKey.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHAliceKey.TabIndex = 7
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(18, 154)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(25, 13)
        Me.Label13.TabIndex = 6
        Me.Label13.Text = "Key"
        '
        'btnECDHBobGenParams
        '
        Me.btnECDHBobGenParams.Location = New System.Drawing.Point(175, 81)
        Me.btnECDHBobGenParams.Name = "btnECDHBobGenParams"
        Me.btnECDHBobGenParams.Size = New System.Drawing.Size(106, 23)
        Me.btnECDHBobGenParams.TabIndex = 5
        Me.btnECDHBobGenParams.Text = "Generate Params"
        Me.btnECDHBobGenParams.UseVisualStyleBackColor = True
        '
        'btnECDHAliceGenParams
        '
        Me.btnECDHAliceGenParams.Location = New System.Drawing.Point(175, 12)
        Me.btnECDHAliceGenParams.Name = "btnECDHAliceGenParams"
        Me.btnECDHAliceGenParams.Size = New System.Drawing.Size(106, 23)
        Me.btnECDHAliceGenParams.TabIndex = 4
        Me.btnECDHAliceGenParams.Text = "Generate Params"
        Me.btnECDHAliceGenParams.UseVisualStyleBackColor = True
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(18, 86)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(26, 13)
        Me.Label12.TabIndex = 3
        Me.Label12.Text = "Bob"
        '
        'tbECDHBob
        '
        Me.tbECDHBob.Location = New System.Drawing.Point(69, 83)
        Me.tbECDHBob.Name = "tbECDHBob"
        Me.tbECDHBob.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHBob.TabIndex = 2
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(18, 17)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(30, 13)
        Me.Label11.TabIndex = 1
        Me.Label11.Text = "Alice"
        '
        'tbECDHAlice
        '
        Me.tbECDHAlice.Location = New System.Drawing.Point(69, 14)
        Me.tbECDHAlice.Name = "tbECDHAlice"
        Me.tbECDHAlice.Size = New System.Drawing.Size(100, 20)
        Me.tbECDHAlice.TabIndex = 0
        '
        'tpTest
        '
        Me.tpTest.Controls.Add(Me.pbTest)
        Me.tpTest.Controls.Add(Me.btnTestStart)
        Me.tpTest.Location = New System.Drawing.Point(4, 22)
        Me.tpTest.Name = "tpTest"
        Me.tpTest.Padding = New System.Windows.Forms.Padding(3)
        Me.tpTest.Size = New System.Drawing.Size(370, 211)
        Me.tpTest.TabIndex = 3
        Me.tpTest.Text = "Stresstest"
        Me.tpTest.UseVisualStyleBackColor = True
        '
        'pbTest
        '
        Me.pbTest.Location = New System.Drawing.Point(6, 182)
        Me.pbTest.Name = "pbTest"
        Me.pbTest.Size = New System.Drawing.Size(277, 23)
        Me.pbTest.TabIndex = 1
        '
        'btnTestStart
        '
        Me.btnTestStart.Location = New System.Drawing.Point(289, 182)
        Me.btnTestStart.Name = "btnTestStart"
        Me.btnTestStart.Size = New System.Drawing.Size(75, 23)
        Me.btnTestStart.TabIndex = 0
        Me.btnTestStart.Text = "Start"
        Me.btnTestStart.UseVisualStyleBackColor = True
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(402, 261)
        Me.Controls.Add(Me.mainTabControl)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "frmMain"
        Me.Text = "CryptTestApplication"
        Me.mainTabControl.ResumeLayout(False)
        Me.tpRSA.ResumeLayout(False)
        Me.tpRSA.PerformLayout()
        Me.tpSHA.ResumeLayout(False)
        Me.tpSHA.PerformLayout()
        Me.gbShaMode.ResumeLayout(False)
        Me.gbShaMode.PerformLayout()
        Me.tpECDH.ResumeLayout(False)
        Me.tpECDH.PerformLayout()
        Me.tpTest.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents mainTabControl As TabControl
    Friend WithEvents tpRSA As TabPage
    Friend WithEvents tpSHA As TabPage
    Friend WithEvents tbShaHashHash As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents tbShaHash As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents tbShaPlainText As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents tbRsaPublicKey As TextBox
    Friend WithEvents tbRsaPrivateKey As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents tbRsaCiphertext As TextBox
    Friend WithEvents Label9 As Label
    Friend WithEvents tbRsaPlaintext As TextBox
    Friend WithEvents btnRsaExtract As Button
    Friend WithEvents btnRsaGenerate As Button
    Friend WithEvents btnRsaEncrypt As Button
    Friend WithEvents btnRsaDecrypt As Button
    Friend WithEvents gbShaMode As GroupBox
    Friend WithEvents rbShaModeHex As RadioButton
    Friend WithEvents rbShaModeUTF8 As RadioButton
    Friend WithEvents tpTest As TabPage
    Friend WithEvents pbTest As ProgressBar
    Friend WithEvents btnTestStart As Button
    Friend WithEvents tpECDH As TabPage
    Friend WithEvents btnECDHCreateKey As Button
    Friend WithEvents tbECDHAliceKey As TextBox
    Friend WithEvents Label13 As Label
    Friend WithEvents btnECDHBobGenParams As Button
    Friend WithEvents btnECDHAliceGenParams As Button
    Friend WithEvents Label12 As Label
    Friend WithEvents tbECDHBob As TextBox
    Friend WithEvents Label11 As Label
    Friend WithEvents tbECDHAlice As TextBox
    Friend WithEvents Label16 As Label
    Friend WithEvents tbECDHBobPub As TextBox
    Friend WithEvents Label15 As Label
    Friend WithEvents tbECDHAlicePub As TextBox
    Friend WithEvents tbECDHBobKey As TextBox
End Class
