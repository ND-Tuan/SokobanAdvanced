
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerDataManager
{
    private PlayerData playerData = new();
    public PlayerData PlayerData => playerData;

    private Dictionary<int, SkinSO> skinDataDictionary = new();
    public Dictionary<int, SkinSO> SkinDataDictionary => skinDataDictionary;

    private int maxEnergy;


    public async Task InitializeAsync(int coinDefaultAmount, int MaxEnergy)
    {

        await Load(coinDefaultAmount, MaxEnergy);
        maxEnergy = MaxEnergy;
    }

    //====Xử lý dữ liệu xu=========================
    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        playerData.Coin += amount;
        SaveSystem.SavePlayerData(playerData);
    }

    public bool CheckCoinEnough(int amount)
    {
        return amount <= playerData.Coin;
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0 || playerData.Coin < amount) return false;
        playerData.Coin -= amount;
        SaveSystem.SavePlayerData(playerData);
        return true;
    }


    //====Xử lý dữ liệu năng lượng=========================
    public void AddEnergy(int amount)
    {
        if (amount <= 0) return;
        playerData.Energy += amount;
        SaveSystem.SavePlayerData(playerData);
    }

    public bool SpendEnergy(int amount)
    {
        playerData.Energy -= amount;
        SaveSystem.SavePlayerData(playerData);
        return true;
    }

    public void SaveEnergyTimer(DateTime lastTime, float time)
    {
        playerData.LastQuitTime = lastTime;
        playerData.EnergyTimer = time;
        SaveSystem.SavePlayerData(playerData);
    }
    
    //====Xử lý dữ liệu cài đặt=========================
    public void SaveSettingsData(float musicVolume, float sfxVolume)
    {
        playerData.MusicVolume = musicVolume;
        playerData.SFXVolume = sfxVolume;

        SaveSystem.SavePlayerData(playerData);
    }


    //====Xử lý dữ liệu skin=========================

    public void BuySkin(int skinID)
    {
        if (CheckSkinOwned(skinID)) return;

        if (CheckCoinEnough(skinDataDictionary[skinID].Price))
        {
            SpendCoin(skinDataDictionary[skinID].Price);
            playerData.SkinOwned.Add(skinID);
            SaveSystem.SavePlayerData(playerData);
        }
    }

    public void EquipSkin(int skinID)
    {
        if (CheckSkinOwned(skinID))
        {
            playerData.SkinEquipped = skinID;
            SaveSystem.SavePlayerData(playerData);
        }
    }

    public SkinSO GetEquippedSkin()
    {
        if (skinDataDictionary.TryGetValue(playerData.SkinEquipped, out var skin))
        {
            return skin;
        }
        return null;
    }

    public bool CheckSkinOwned(int skinID)
    {
        return playerData.SkinOwned.Contains(skinID);
    }


    //====Xử lý dữ liệu nhiệm vụ=========================
    public void SaveDailyTaskData(Dictionary<TaskType, int> dailyTasks)
    {
        List<int> taskIDs = new List<int>();
        List<int> taskProgress = new List<int>();

        foreach (var task in dailyTasks)
        {
            taskIDs.Add((int)task.Key);
            taskProgress.Add(task.Value);
        }

        // Update player data with the new tasks
        playerData.DailyTaskID = taskIDs;
        playerData.DailyTaskProgress = taskProgress;
        

        SaveSystem.SavePlayerData(playerData);
    }

    public void ClaimReward()
    {
       
        playerData.DailyClaimed += 1;
        SaveSystem.SavePlayerData(playerData);
    }

    public void RestClaimed()
    {
        playerData.DailyClaimed = 0;
        SaveSystem.SavePlayerData(playerData);
    }
    

    public void Save()
    {
        SaveSystem.SavePlayerData(playerData);
    }


    private async Task Load(int coinDefaultAmount, int MaxEnergy)
    {
        // Load player data from file
        playerData = await SaveSystem.LoadPlayerData();

        if (playerData == null)
        {
            playerData = new PlayerData
            {
                Coin = coinDefaultAmount,
                Energy = MaxEnergy,
                SkinOwned = new List<int> { 0 },
                SkinEquipped = 0,
                MusicVolume = 1f,
                SFXVolume = 1f,
                LastQuitTime = DateTime.Now,
                EnergyTimer = 0f,

                DailyTaskID = new List<int>(),
                DailyTaskProgress = new List<int>(),
                DailyClaimed = 0

            };
        }
    }
    
    public async Task LoadSkinDataAsync()
    {
        var handle = Addressables.LoadAssetsAsync<SkinSO>("SkinData", null);
        var result = await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var skin in result)
            {
                skinDataDictionary[skin.SkinID] = skin;
                Debug.Log($"Loaded skin: {skin.SkinName}");
            }
        }
        else
        {
            Debug.LogError("Failed to load skin data from Addressables!");
        }
    }
}
