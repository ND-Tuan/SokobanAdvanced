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

    //====Load/Unload Level================
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
    
    //Tải lại level hiện tại
    public void ReloadLevel()
    {

        if (currentMapManager == null)
        {
            Debug.LogError("Current Map Manager is not set. Cannot reload level.");
            return;
        }
        currentMapManager.ResetMap();
    }

    //Tải level tiếp theo
    public void LoadNextLevel()
    {   
        //Xác định key của level tiếp theo
        var nextLevelKey = (CurrentLevel.Stage, CurrentLevel.Index + 1);

        //Kiểm tra nếu level tiếp theo không tồn tại
        if (!levelStatusValue.ContainsKey(nextLevelKey))
        {
            UIController.Instance.OnClickBackToLevelSelect();
            return;
        }
        
        UnloadLevel();
        LoadLevel(nextLevelKey.Stage, nextLevelKey.Item2);
    }

    //Unload level hiện tại
    public async void UnloadLevel()
    {
        if (currentSceneHandle.HasValue)
        {
            await Addressables.UnloadSceneAsync(currentSceneHandle.Value).Task;
            currentSceneHandle = null;
        }
    }

    //Lấy giá trị trạng thái của level
    public int GetLevelStatusValue(int stage, int levelIndex)
    {
        return levelStatusValue.TryGetValue((stage, levelIndex), out int value) ? value : -1;
    }


    //=========Setup Map================
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

    //Giảm số lượt di chuyển
    public void MinusMoveCount()
    {
        MoveCount--;
        UIController.Instance.SetMoveCountUI(MoveCount, MoveCountLimit, moveToGetStar);

        // if (MoveCount <= 0)
        //     GameManager.Instance.GameOver();
    }

    //Tính số sao đạt được
    public int CalculateStar() =>
        moveToGetStar.Count(threshold => MoveCount >= threshold);


     //Cập nhật trạng thái của level
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

    //Mở khóa level tiếp theo
    public void UnlockNextLevel(int NumberOfLevelsAvailable)
    {
        //Kiểm tra nếu tất cả các level có sẵn đã được mở khóa
        if (levelStatusValue.Count >= NumberOfLevelsAvailable)
            return;

        var nextKey = (CurrentLevel.Stage, CurrentLevel.Index + 1);
        
        //Nếu level tiếp theo đã được mở khóa, không làm gì cả
        if (levelStatusValue.ContainsKey(nextKey))
            return;

        //Kiểm tra nếu hộp chọn level đã được đăng ký
        if (levelSelectBoxes.ContainsKey(nextKey))
        {
            levelStatusValue.Add(nextKey, 0);
            return;
        }
        
        //Tiếp tục kiểm tra stage tiếp theo
        nextKey = (CurrentLevel.Stage + 1, 1);
        
        //Nếu level tiếp theo đã được mở khóa, không làm gì cả
        if (levelStatusValue.ContainsKey(nextKey))
            return;

        if (levelSelectBoxes.ContainsKey(nextKey))
        {
            levelStatusValue.Add(nextKey, 0);
            UIController.Instance.NextStage();
            return;
        }
    }

    //Kiểm tra xem có đủ năng lượng để chơi level không
    public bool CheckEnergyAvailable()
    {
        if (GameManager.Instance.PlayerDataManager.PlayerData.Energy > 0)
            return true;

        //Nếu không đủ năng lượng, hiển thị thông báo
        Debug.Log("Not enough energy to play the level.");
        return false;
    }
    
    //Lưu trạng thái level vào file
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
