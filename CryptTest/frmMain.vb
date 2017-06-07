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
    Private Sub btnAesGenerate_Click(sender As Object, e As EventArgs) Handles btnAesGenerate.Click
        tbAesKey.Text = Util.ToHexString(AES.GenerateKey())
    End Sub

    Private Sub btnAesGenerateIV_Click(sender As Object, e As EventArgs) Handles btnAesGenerateIV.Click
        tbAesIV.Text = Util.ToHexString(AES.GenerateIV())
    End Sub

    Private Async Sub btnAesDecrypt_Click(sender As Object, e As EventArgs) Handles btnAesDecrypt.Click
        Dim enc As New Text.UTF8Encoding
        Dim ct As Byte() = Util.GetBytes(tbAesCipherText.Text)
        Dim pt As Byte() = Await AES.DecryptAsync(ct, Util.GetBytes(tbAesKey.Text), If(String.IsNullOrEmpty(tbAesIV.Text), Nothing, Util.GetBytes(tbAesIV.Text)))
        'tbAesPlainText.Text = enc.GetString(pt)
        tbAesPlainText.Text = Util.ToHexString(pt)
        tbAesCipherText.Text = ""
    End Sub

    Private Async Sub btnAesEncrypt_Click(sender As Object, e As EventArgs) Handles btnAesEncrypt.Click
        Dim enc As New Text.UTF8Encoding
        'Dim pt As Byte() = enc.GetBytes(tbAesPlainText.Text)
        Dim pt As Byte() = Util.GetBytes(tbAesPlainText.Text)
        Dim ct As Byte() = Await AES.EncryptAsync(pt, Util.GetBytes(tbAesKey.Text), If(String.IsNullOrEmpty(tbAesIV.Text), Nothing, Util.GetBytes(tbAesIV.Text)))
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
        'tbECDHAlice.Text = Util.GetHexString(ECDH.GeneratePrivateKey())
        Dim a As String
        Dim b As String
        ECDH.Deserialize(a, b)
        MsgBox(a & b)
    End Sub
#End Region
#Region "Test"
    Private Async Sub btnTestStart_Click(sender As Object, e As EventArgs) Handles btnTestStart.Click
        Dim key As String = Await Task.Run(Function() RSA.GenerateKeyPair())
        For i As Integer = 0 To 10000
            Await RSA.EncryptAsync({223, 25, 84}, key)
            pbTest.Value = CInt(i / 100)
        Next
    End Sub
#End Region
End Class
Public Class ECDH
    Public Shared Function GeneratePrivateKey() As String
        Dim privateKey As String
        Using ecdh As New Security.Cryptography.ECDiffieHellmanCng(256)
            ecdh.KeyDerivationFunction = Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Hash
            ecdh.HashAlgorithm = Security.Cryptography.CngAlgorithm.Sha256
            privateKey = ecdh.PublicKey.ToXmlString()
        End Using
        Return privateKey
    End Function
    Public Shared Sub Deserialize(ByRef ecdhxml As String, ByRef publickey As String)
        Dim cng As Security.Cryptography.CngKey = Security.Cryptography.CngKey.Create(Security.Cryptography.CngAlgorithm.ECDiffieHellmanP256, Nothing, New Security.Cryptography.CngKeyCreationParameters() With {.ExportPolicy = Security.Cryptography.CngExportPolicies.AllowPlaintextExport})
        Using ecdh As New Security.Cryptography.ECDiffieHellmanCng(cng)
            ecdh.KeyDerivationFunction = Security.Cryptography.ECDiffieHellmanKeyDerivationFunction.Hash
            ecdh.HashAlgorithm = Security.Cryptography.CngAlgorithm.Sha256
            ecdhxml = Util.ToHexString(ecdh.Key.Export(Security.Cryptography.CngKeyBlobFormat.EccPrivateBlob))
            publickey = ecdh.PublicKey.ToXmlString()
        End Using
    End Sub
End Class