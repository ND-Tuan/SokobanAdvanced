using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
public static class AESUtil
{   
    //Mã hóa dữ liệu đầu vào sử dụng AES với KeyContainer đã cho
    public static byte[] Encrypt(byte[] input, KeyContainer container)
    {
        //Tạo IV mới cho mỗi lần mã hóa
        byte[] iv = container.GetNewIV();

        //Sử dụng AES để mã hóa
        using SymmetricAlgorithm algorithm = Aes.Create();
        using ICryptoTransform encryptor = algorithm.CreateEncryptor(container.GetKey(), iv);

        //Kết hợp IV và dữ liệu đã mã hóa vào một mảng byte duy nhất
        List<byte> output = new(iv.Length + input.Length + algorithm.BlockSize / 8);
        byte[] encryptedInput = encryptor.TransformFinalBlock(input, 0, input.Length);

        //Thêm IV vào đầu mảng đầu ra
        output.AddRange(iv);
        output.AddRange(encryptedInput);

        //Xóa thông tin nhạy cảm khỏi bộ nhớ
        algorithm.Clear();

        return output.ToArray();
    }

    //Giải mã dữ liệu đầu vào sử dụng AES với KeyContainer đã cho
    public static byte[] Decrypt(byte[] input, KeyContainer container)
    {
        //Tách IV và dữ liệu đã mã hóa từ mảng byte đầu vào
        byte[] iv = input[..KeyContainer.IVLength];
        byte[] encryptedInput = input[KeyContainer.IVLength..];

        //Sử dụng AES để giải mã
        using SymmetricAlgorithm algorithm = Aes.Create();
        using ICryptoTransform decryptor = algorithm.CreateDecryptor(container.GetKey(), iv);

        //Giải mã dữ liệu
        byte[] output = decryptor.TransformFinalBlock(encryptedInput, 0, encryptedInput.Length);

        //Xóa thông tin nhạy cảm khỏi bộ nhớ
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
