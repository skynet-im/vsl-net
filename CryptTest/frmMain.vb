Imports System.Text
Imports VSL.Crypt
Public Class frmMain
#Region "RSA"
    Private Async Sub btnRsaExtract_Click(sender As Object, e As EventArgs) Handles btnRsaExtract.Click
        If tbRsaPrivateKey.Text = "" Then tbRsaPrivateKey.Text = Await Task.Run(Function() RSA.GenerateKeyPairXml())
        tbRsaPublicKey.Text = RSA.ExtractPublicKey(tbRsaPrivateKey.Text)
    End Sub
    Private Async Sub btnRsaGenerate_Click(sender As Object, e As EventArgs) Handles btnRsaGenerate.Click
        tbRsaPrivateKey.Text = Await Task.Run(Function() RSA.GenerateKeyPairXml())
    End Sub
    Private Async Sub btnRsaEncrypt_Click(sender As Object, e As EventArgs) Handles btnRsaEncrypt.Click
        Dim enc As New Text.UTF8Encoding()
        Dim pt As Byte() = enc.GetBytes(tbRsaPlaintext.Text)
        Dim ct As Byte() = Await Task.Run(Function() RSA.Encrypt(pt, tbRsaPublicKey.Text))
        tbRsaCiphertext.Text = Util.ToHexString(ct)
        tbRsaPlaintext.Text = ""
    End Sub
    Private Async Sub btnRsaDecrypt_Click(sender As Object, e As EventArgs) Handles btnRsaDecrypt.Click
        Dim enc As New Text.UTF8Encoding()
        Dim ct As Byte() = Util.GetBytes(tbRsaCiphertext.Text)
        Dim pt As Byte() = Await Task.Run(Function() RSA.Decrypt(ct, tbRsaPrivateKey.Text))
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

    Private Sub btnAesDecrypt_Click(sender As Object, e As EventArgs) Handles btnAesDecrypt.Click
        If decryptCsp Is Nothing Then
            decryptCsp = New AesCsp(Util.GetBytes(tbAesKey.Text), Util.GetBytes(tbAesIV.Text))
        End If
        Dim ct As Byte() = Util.GetBytes(tbAesCipherText.Text)
        Dim pt As Byte() = New Byte(ct.Length - 1) {}
        Dim last As Integer = ct.Length Mod 16
        If last = 0 Then last = 16
        Dim between As Integer = ct.Length - last
        Using aes As New Security.Cryptography.AesCryptoServiceProvider()
            aes.Key = Util.GetBytes(tbAesKey.Text)
            aes.IV = Util.GetBytes(tbAesIV.Text)
            Using trans As Security.Cryptography.ICryptoTransform = aes.CreateDecryptor()
                Dim length As Integer = trans.TransformBlock(ct, 0, 16, pt, 0)
                length += trans.TransformBlock(ct, 16, 16, pt, 0)
                Dim lastB As Byte() = trans.TransformFinalBlock(ct, 16, ct.Length - length)
                pt = Util.TakeBytes(pt, length)
                pt = Util.ConnectBytes(pt, lastB)
            End Using
        End Using
        'Dim pt As Byte() = decryptCsp.Decrypt(ct)
        If EncodingUTF8Rb.Checked Then
            tbAesPlainText.Text = Encoding.UTF8.GetString(pt)
        Else
            tbAesPlainText.Text = Util.ToHexString(pt)
        End If
        tbAesCipherText.Text = ""
    End Sub

    Private Async Sub btnAesEncrypt_Click(sender As Object, e As EventArgs) Handles btnAesEncrypt.Click
        If encryptCsp Is Nothing Then
            encryptCsp = New AesCsp(Util.GetBytes(tbAesKey.Text), Util.GetBytes(tbAesIV.Text))
        End If
        Dim pt As Byte()
        If EncodingUTF8Rb.Checked Then
            pt = Encoding.UTF8.GetBytes(tbAesPlainText.Text)
        Else
            pt = Util.GetBytes(tbAesPlainText.Text)
        End If
        Dim ct As Byte() = Await encryptCsp.EncryptAsync(pt)
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