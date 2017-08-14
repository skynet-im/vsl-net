Imports VSL.Crypt
Public Class frmMain
#Region "RSA"
    Private Async Sub btnRsaExtract_Click(sender As Object, e As EventArgs) Handles btnRsaExtract.Click
        If tbRsaPrivateKey.Text = "" Then tbRsaPrivateKey.Text = Await Task.Run(Function() RSA.GenerateKeyPair())
        tbRsaPublicKey.Text = RSA.ExtractPublicKey(tbRsaPrivateKey.Text)
    End Sub
    Private Async Sub btnRsaGenerate_Click(sender As Object, e As EventArgs) Handles btnRsaGenerate.Click
        tbRsaPrivateKey.Text = Await Task.Run(Function() RSA.GenerateKeyPair())
    End Sub
    Private Async Sub btnRsaEncrypt_Click(sender As Object, e As EventArgs) Handles btnRsaEncrypt.Click
        Dim enc As New Text.UTF8Encoding()
        Dim pt As Byte() = enc.GetBytes(tbRsaPlaintext.Text)
        Dim ct As Byte() = Await RSA.EncryptAsync(pt, tbRsaPublicKey.Text)
        tbRsaCiphertext.Text = Util.ToHexString(ct)
        tbRsaPlaintext.Text = ""
    End Sub
    Private Async Sub btnRsaDecrypt_Click(sender As Object, e As EventArgs) Handles btnRsaDecrypt.Click
        Dim enc As New Text.UTF8Encoding()
        Dim ct As Byte() = Util.GetBytes(tbRsaCiphertext.Text)
        Dim pt As Byte() = Await RSA.DecryptAsync(ct, tbRsaPrivateKey.Text)
        tbRsaPlaintext.Text = enc.GetString(pt)
        tbRsaCiphertext.Text = ""
    End Sub
#End Region
#Region "AES"
    Private encryptCsp As AesCsp
    Private decryptCsp As AesCsp
    Private Sub btnAesGenerate_Click(sender As Object, e As EventArgs) Handles btnAesGenerate.Click
        tbAesKey.Text = Util.ToHexString(AES.GenerateKey())
    End Sub

    Private Sub btnAesGenerateIV_Click(sender As Object, e As EventArgs) Handles btnAesGenerateIV.Click
        tbAesIV.Text = Util.ToHexString(AES.GenerateIV())
    End Sub

    Private Async Sub btnAesDecrypt_Click(sender As Object, e As EventArgs) Handles btnAesDecrypt.Click
        Dim enc As New Text.UTF8Encoding
        Dim ct As Byte() = Util.GetBytes(tbAesCipherText.Text)
        'Dim pt As Byte() = Await AES.DecryptAsync(ct, Util.GetBytes(tbAesKey.Text), If(String.IsNullOrEmpty(tbAesIV.Text), Nothing, Util.GetBytes(tbAesIV.Text)))
        Dim pt As Byte()
        If decryptCsp Is Nothing Then
            decryptCsp = New AesCsp(Util.GetBytes(tbAesKey.Text), Util.GetBytes(tbAesIV.Text))
        End If
        pt = Await decryptCsp.DecryptAsync(ct)
        'tbAesPlainText.Text = enc.GetString(pt)
        tbAesPlainText.Text = Util.ToHexString(pt)
        tbAesCipherText.Text = ""
    End Sub

    Private Async Sub btnAesEncrypt_Click(sender As Object, e As EventArgs) Handles btnAesEncrypt.Click
        Dim enc As New Text.UTF8Encoding
        'Dim pt As Byte() = enc.GetBytes(tbAesPlainText.Text)
        Dim pt As Byte() = Util.GetBytes(tbAesPlainText.Text)
        'Dim ct As Byte() = Await AES.EncryptAsync(pt, Util.GetBytes(tbAesKey.Text), If(String.IsNullOrEmpty(tbAesIV.Text), Nothing, Util.GetBytes(tbAesIV.Text)))
        Dim ct As Byte()
        If encryptCsp Is Nothing Then
            encryptCsp = New AesCsp(Util.GetBytes(tbAesKey.Text), Util.GetBytes(tbAesIV.Text))
        End If
        ct = Await encryptCsp.EncryptAsync(pt)
        tbAesCipherText.Text = Util.ToHexString(ct)
        tbAesPlainText.Text = ""
    End Sub
#End Region
#Region "SHA"
    Private Sub tbShaPlainText_TextChanged(sender As Object, e As EventArgs) Handles tbShaPlainText.TextChanged
        Try
            Dim text As Byte()
            If rbShaModeUTF8.Checked Then
                Dim enc As New Text.UTF8Encoding()
                text = enc.GetBytes(tbShaPlainText.Text)
            Else
                If String.IsNullOrWhiteSpace(tbShaPlainText.Text) Then
                    text = New Byte(-1) {}
                Else
                    text = Util.GetBytes(tbShaPlainText.Text)
                End If
            End If
            tbShaHash.Text = Util.ToHexString(Hash.SHA256(text))
            tbShaHashHash.Text = Util.ToHexString(Hash.SHA256(Hash.SHA256(text)))
        Catch ex As Exception
        End Try
    End Sub

    Private Sub rbShaModeUTF8_CheckedChanged(sender As Object, e As EventArgs) Handles rbShaModeUTF8.CheckedChanged
        If rbShaModeUTF8.Checked Then
            tbShaPlainText.UseSystemPasswordChar = True
        End If
    End Sub

    Private Sub rbShaModeHex_CheckedChanged(sender As Object, e As EventArgs) Handles rbShaModeHex.CheckedChanged
        If rbShaModeHex.Checked Then
            tbShaPlainText.UseSystemPasswordChar = False
        End If
    End Sub
#End Region
#Region "ECDH"
    Private Sub btnECDHAliceGenParams_Click(sender As Object, e As EventArgs) Handles btnECDHAliceGenParams.Click
        Dim prv As Byte() = {}
        Dim pub As Byte() = {}
        ECDH.GenerateKey(prv, pub)
        tbECDHAlice.Text = Util.ToHexString(prv)
        tbECDHAlicePub.Text = Util.ToHexString(pub)
    End Sub
    Private Sub btnECDHBobGenParams_Click(sender As Object, e As EventArgs) Handles btnECDHBobGenParams.Click
        Dim prv As Byte() = {}
        Dim pub As Byte() = {}
        ECDH.GenerateKey(prv, pub)
        tbECDHBob.Text = Util.ToHexString(prv)
        tbECDHBobPub.Text = Util.ToHexString(pub)
    End Sub
    Private Sub btnECDHCreateKey_Click(sender As Object, e As EventArgs) Handles btnECDHCreateKey.Click
        tbECDHAliceKey.Text = Util.ToHexString(ECDH.DeriveKey(Util.GetBytes(tbECDHAlice.Text), Util.GetBytes(tbECDHBobPub.Text)))
        tbECDHBobKey.Text = Util.ToHexString(ECDH.DeriveKey(Util.GetBytes(tbECDHBob.Text), Util.GetBytes(tbECDHAlicePub.Text)))
    End Sub
#End Region
#Region "Test"
    Private Sub btnTestStart_Click(sender As Object, e As EventArgs) Handles btnTestStart.Click
        Dim ran As New Random()
        Dim b(15) As Byte
        ran.NextBytes(b)
        Dim c As Byte() = Security.Cryptography.ProtectedData.Protect(b, Nothing, Security.Cryptography.DataProtectionScope.CurrentUser)
        MsgBox(c.Length)
        'Dim key As String = Await Task.Run(Function() RSA.GenerateKeyPair())
        'For i As Integer = 0 To 10000
        '    Await RSA.EncryptAsync({223, 25, 84}, key)
        '    pbTest.Value = CInt(i / 100)
        'Next
    End Sub
#End Region
End Class