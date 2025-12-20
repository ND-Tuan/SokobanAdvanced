using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

[Serializable]
public class PlayerData
{
    public int Coin;
    public int Energy;
    public List<int> SkinOwned;
    public int SkinEquipped;
    public float MusicVolume;
    public float SFXVolume;

    public string LastQuitTimeString;
    public float EnergyTimer;

    public List<int> DailyTaskID;
    public List<int> DailyTaskProgress;
    public int DailyClaimed;

    public DateTime LastQuitTime
    {
        get => DateTime.Parse(LastQuitTimeString);
        set => LastQuitTimeString = value.ToString("o");
    }
}

[Serializable]
public class LevelEntry
{
    public int Stage;
    public int Level;
    public int Status;
}

[Serializable]
public class LevelData
{
    public List<LevelEntry> Entries = new List<LevelEntry>();

    public LevelData(Dictionary<(int, int), int> levelStatusValue)
    {
        Entries = levelStatusValue.Select(kv => new LevelEntry
        {
            Stage = kv.Key.Item1,
            Level = kv.Key.Item2,
            Status = kv.Value
        }).ToList();
    }

    public Dictionary<(int, int), int> ToDictionary()
    {
        return Entries.ToDictionary(e => (e.Stage, e.Level), e => e.Status);
    }
}

public static class SaveSystem
{
    private static readonly string playerDataPath = Path.Combine(Application.persistentDataPath, "playerdata.json");
    private static readonly string levelDataPath = Path.Combine(Application.persistentDataPath, "leveldata.json");

    //=== PUBLIC API ===//

    public static void SavePlayerData(PlayerData data)
    {
        SaveEncryptedJson(playerDataPath, data);
    }

    public static async Task<PlayerData> LoadPlayerData()
    {
        return await Task.Run(() => LoadEncryptedJson<PlayerData>(playerDataPath));
    }

    public static void SaveLevelData(LevelData data)
    {
        SaveEncryptedJson(levelDataPath, data);
    }

    public static LevelData LoadLevelData()
    {
        return LoadEncryptedJson<LevelData>(levelDataPath);
    }

    //=== INTERNAL METHODS ===//

    //KeyContainer dùng để mã hóa và giải mã dữ liệu
    private static readonly KeyContainer keyContainer = new("YourUID");

    //Lưu dữ liệu dạng JSON đã mã hóa vào file
    private static void SaveEncryptedJson<T>(string path, T data)
    {
        try
        {
            //Chuyển đổi dữ liệu thành JSON và mã hóa nó
            string json = JsonUtility.ToJson(data);
            byte[] inputBytes = Encoding.UTF8.GetBytes(json);
            byte[] encryptedBytes = AESUtil.Encrypt(inputBytes, keyContainer);
            File.WriteAllBytes(path, encryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save encrypted data to {path}: {ex.Message}");
        }
    }

    //Tải dữ liệu dạng JSON đã mã hóa từ file
    private static T LoadEncryptedJson<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Encrypted file not found at {path}");
            return null;
        }

        try
        {
            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] decryptedBytes = AESUtil.Decrypt(encryptedBytes, keyContainer);
            string json = Encoding.UTF8.GetString(decryptedBytes);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load encrypted data from {path}: {ex.Message}");
            return null;
        }
    }
}

