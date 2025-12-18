using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
public static class AESUtil
{
    public static byte[] Encrypt(byte[] input, KeyContainer container)
    {
        byte[] iv = container.GetNewIV();

        using SymmetricAlgorithm algorithm = Aes.Create();
        using ICryptoTransform encryptor = algorithm.CreateEncryptor(container.GetKey(), iv);

        List<byte> output = new(iv.Length + input.Length + algorithm.BlockSize / 8);
        byte[] encryptedInput = encryptor.TransformFinalBlock(input, 0, input.Length);

        output.AddRange(iv);
        output.AddRange(encryptedInput);

        algorithm.Clear();

        return output.ToArray();
    }

    public static byte[] Decrypt(byte[] input, KeyContainer container)
    {
        byte[] iv = input[..KeyContainer.IVLength];
        byte[] encryptedInput = input[KeyContainer.IVLength..];

        using SymmetricAlgorithm algorithm = Aes.Create();
        using ICryptoTransform decryptor = algorithm.CreateDecryptor(container.GetKey(), iv);

        byte[] output = decryptor.TransformFinalBlock(encryptedInput, 0, encryptedInput.Length);

        algorithm.Clear();

        return output;
    }
}

public class KeyContainer
{
    private readonly byte[] key;
    public static readonly int IVLength = 16;

    public KeyContainer(string passphrase)
    {
        using var sha = SHA256.Create();
        key = sha.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
    }

    public byte[] GetKey() => key;

    public byte[] GetNewIV()
    {
        byte[] iv = new byte[IVLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);
        return iv;
    }
}
