using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LevelManager
{
    
    private Dictionary<(int, int), int> levelStatusValue = new();
    private Dictionary<(int, int), LevelSelectBox> levelSelectBoxes = new();
    private AsyncOperationHandle<SceneInstance>? currentSceneHandle;

    public LevelID CurrentLevel { get; private set; }

    private string CurrentScenePath => $"Assets/Scenes/Stage_{CurrentLevel.Stage}/{CurrentLevel.Stage}_{CurrentLevel.Index}.unity";


    private MapController currentMapManager;
    public int MoveCount { get; private set; }
    public int MoveCountLimit { get; private set; }
    private int[] moveToGetStar = new int[3];


    public LevelManager()
    {
        Load();
    }

    public void RegisterLevelSelectBox(int stage, int levelIndex, LevelSelectBox levelSelectBox)
    {
        levelSelectBoxes[(stage, levelIndex)] = levelSelectBox;
    }

    //====Load a level================
    public async void LoadLevel(int stage, int level)
    {

        CurrentLevel = new LevelID { Stage = stage, Index = level };

        var handle = Addressables.LoadSceneAsync(CurrentScenePath, LoadSceneMode.Additive);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded){
            currentSceneHandle = handle;
        } else {
            Debug.LogError($"Failed to load scene at address: {CurrentScenePath}");
        }
    }
    
    public void ReloadLevel()
    {

        if (currentMapManager == null)
        {
            Debug.LogError("Current Map Manager is not set. Cannot reload level.");
            return;
        }
        currentMapManager.ResetMap();
    }

    public void LoadNextLevel()
    {   

        var nextLevelKey = (CurrentLevel.Stage, CurrentLevel.Index + 1);

        if (!levelStatusValue.ContainsKey(nextLevelKey))
        {
            UIController.Instance.OnClickBackToLevelSelect();
            return;
        }
        
        UnloadLevel();
        LoadLevel(nextLevelKey.Stage, nextLevelKey.Item2);
    }

    public async void UnloadLevel()
    {
        if (currentSceneHandle.HasValue)
        {
            await Addressables.UnloadSceneAsync(currentSceneHandle.Value).Task;
            currentSceneHandle = null;
        }
    }

    public int GetLevelStatusValue(int stage, int levelIndex)
    {
        return levelStatusValue.TryGetValue((stage, levelIndex), out int value) ? value : -1;
    }


    //====Initialize level parameters================
    public void SetUpMap(MapController mapManager,int moveCountLimit, int[] moveToGetStar)
    {
        currentMapManager = mapManager;
        MoveCount = moveCountLimit;
        MoveCountLimit = moveCountLimit;
        this.moveToGetStar = moveToGetStar;

        //Initialize UI
        UIController.Instance.ShowInPlayLevelUI();
        UIController.Instance.SetMoveCountUI(MoveCount, MoveCountLimit, moveToGetStar);
    }

    //=========Calculate remaining moves================
    public void MinusMoveCount()
    {
        MoveCount--;
        UIController.Instance.SetMoveCountUI(MoveCount, MoveCountLimit, moveToGetStar);

        // if (MoveCount <= 0)
        //     GameManager.Instance.GameOver();
    }

    //Calculate the number of stars achieved
    public int CalculateStar() =>
        moveToGetStar.Count(threshold => MoveCount >= threshold);


     //Update the status of the level
    public int UpdateLevelStatus(int star)
    {
        var key = (CurrentLevel.Stage, CurrentLevel.Index);
        int starAmountDifference = 0;

        if (levelStatusValue.ContainsKey(key))
        {
            if(star > levelStatusValue[key])
                starAmountDifference = star - levelStatusValue[key];

            levelStatusValue[key] = Mathf.Max(levelStatusValue[key], star);
            
        } else {
            levelStatusValue.Add(key, star);
            starAmountDifference = star;
        }

        return starAmountDifference;
    }

    //Unlock the next level
    public void UnlockNextLevel(int NumberOfLevelsAvailable)
    {
        //Check if all available levels have already been unlocked
        if (levelStatusValue.Count >= NumberOfLevelsAvailable)
            return;

        var nextKey = (CurrentLevel.Stage, CurrentLevel.Index + 1);
        
        //If the next level is already unlocked, do nothing
        if (levelStatusValue.ContainsKey(nextKey))
            return;

        //Check if the level select box has been registered
        if (levelSelectBoxes.ContainsKey(nextKey))
        {
            levelStatusValue.Add(nextKey, 0);
            return;
        }
        
        //Continue to check the next stage
        nextKey = (CurrentLevel.Stage + 1, 1);
        
        //If the next level is already unlocked, do nothing
        if (levelStatusValue.ContainsKey(nextKey))
            return;

        if (levelSelectBoxes.ContainsKey(nextKey))
        {
            levelStatusValue.Add(nextKey, 0);
            UIController.Instance.NextStage();
            return;
        }
    }

    //Check if there is enough energy to play the level
    public bool CheckEnergyAvailable()
    {
        if (GameManager.Instance.PlayerDataManager.PlayerData.Energy > 0)
            return true;

        //If not enough energy, show a notification
        Debug.Log("Not enough energy to play the level.");
        return false;
    }
    
    //Save level data
    public void Save() => SaveSystem.SaveLevelData(new LevelData(levelStatusValue));

    private void Load()
    {
        var levelData = SaveSystem.LoadLevelData();
        levelStatusValue = levelData?.ToDictionary();
        
        if (levelStatusValue == null || levelStatusValue.Count == 0)
        {
            levelStatusValue = new Dictionary<(int, int), int> { { (1, 1), 0 } };
        }
    }
}
