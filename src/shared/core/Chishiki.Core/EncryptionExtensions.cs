// -----------------------------------------------------------------------------
// File:        EncryptionExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for AES-based string encryption and decryption.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Security.Cryptography;
using System.Text;

namespace Chishiki;

/// <summary>Extension methods for AES-based string encryption and decryption.</summary>
public static class EncryptionExtension
{
    /// <summary>Encrypts a string using AES encryption.</summary>
    /// <param name="dataToEncrypt">The string to encrypt.</param>
    /// <param name="key">The encryption key used for AES encryption.</param>
    /// <returns>The encrypted string encoded in Base64 format, or null if the input data is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the encryption key is null.</exception>
    public static string? Encrypt(this string dataToEncrypt, string key)
    {
        ArgumentNullException.ThrowIfNull(key, "Encryption key");

        if (string.IsNullOrEmpty(dataToEncrypt) || string.IsNullOrWhiteSpace(dataToEncrypt))
        {
            return null;
        }

        byte[] encrypted;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Padding = PaddingMode.PKCS7;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(dataToEncrypt);
            }

            encrypted = msEncrypt.ToArray();
        }

        return Convert.ToBase64String(encrypted);
    }

    /// <summary>Decrypts a Base64-encoded encrypted string using AES decryption.</summary>
    /// <param name="dataToDecrypt">The encrypted string in Base64 format to decrypt.</param>
    /// <param name="key">The encryption key used for AES decryption.</param>
    /// <returns>The decrypted string, or empty string if the input data is null or empty.</returns>
    public static string Decrypt(this string dataToDecrypt, string key)
    {
        if (string.IsNullOrEmpty(dataToDecrypt) || string.IsNullOrWhiteSpace(dataToDecrypt))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;

        var descriptor = aes.CreateDecryptor(aes.Key, aes.IV);

        var buffer = Convert.FromBase64String(dataToDecrypt);
        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, descriptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }
}
