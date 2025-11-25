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
    }

   
}

public struct LevelID
{
    public int Stage;
    public int Index;
} 

