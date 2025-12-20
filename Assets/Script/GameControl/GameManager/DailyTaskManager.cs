using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ObserverPattern;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DailyTaskManager
{

    public Dictionary<TaskType, TaskSO> AllTasks { get; private set; } = new Dictionary<TaskType, TaskSO>();
    private System.Random rng = new System.Random();
    public Dictionary<TaskType, int> DailyTasksProgress { get; private set; } = new Dictionary<TaskType, int>();

    public int SpecialTileCount {get; private set; } = 0;


    public DailyTaskManager()
    {
        // Đăng ký lắng nghe sự kiện
        Observer.AddListener(EvenID.ClaimDailyTask, OnClaimDailyTask);
        Observer.AddListener(EvenID.ReportTaskProgress, OnReportTaskProgress);
    }

    public async Task InitializeAsync()
    {
        await LoadTasks();
    }

    //Tải tất cả các nhiệm vụ từ Addressables
    private async Task LoadTasks()
    {
        var handle = Addressables.LoadAssetsAsync<TaskSO>("TaskData", null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            AllTasks = handle.Result.ToDictionary(task => task.TaskType);

            if (AllTasks.Count == 0)
            {
                Debug.LogError("No tasks found in Addressables!");
            }
        }
        else
        {
            Debug.LogError("Failed to load tasks from Addressables!");
        }
    }


    //Lấy nhiệm vụ hàng ngày cho người chơi
    public void GetDailyTasks(DateTime now)
    {
        DailyTasksProgress.Clear();

        var playerData = GameManager.Instance.PlayerDataManager.PlayerData;

        // Kiểm tra nếu là ngày mới hoặc chưa có nhiệm vụ nào được lưu
        if ((playerData.DailyTaskID.Count == 0 && playerData.DailyTaskProgress.Count == 0)
            || now.Date != GameManager.Instance.PlayerDataManager.PlayerData.LastQuitTime.Date)
        {
            // Ngày mới - Sinh nhiệm vụ mới
            GetRandomTasks();
            return;
        }

        // Tải nhiệm vụ hiện có từ dữ liệu người chơi
        for (int i = 0; i < playerData.DailyTaskID.Count; i++)
        {
            TaskType taskType = (TaskType)playerData.DailyTaskID[i];
            int progress = playerData.DailyTaskProgress[i];

            DailyTasksProgress.Add(taskType, progress);
            UIController.Instance.UpdateDailyMissionPanel();

        }

    }

    // Sinh nhiệm vụ ngẫu nhiên
    private void GetRandomTasks()
    {   
        // Chia nhiệm vụ thành các nhóm theo độ khó
        List<TaskSO> easyTasks = AllTasks.Values.Where(t => t.PT == 20 && t.TaskType != TaskType.Login).ToList();
        List<TaskSO> mediumTasks = AllTasks.Values.Where(t => t.PT == 30).ToList();
        List<TaskSO> hardTasks = AllTasks.Values.Where(t => t.PT == 40).ToList();

        DailyTasksProgress.Add(TaskType.Login, 1); // Nhiệm vụ đăng nhập luôn có

        // Chọn ngẫu nhiên 2 nhiệm vụ dễ, 2 nhiệm vụ trung bình, 1 nhiệm vụ khó
        AddRandom(easyTasks, 2);
        AddRandom(mediumTasks, 2);
        AddRandom(hardTasks, 1);

        // Đặt lại số lần nhận thưởng hàng ngày
        GameManager.Instance.PlayerDataManager.RestClaimed();
        GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);

        // Cập nhật giao diện
        UIController.Instance.UpdateDailyMissionPanel();
    }

    // Lấy nhiệm vụ ngẫu nhiên từ một danh sách
    private void AddRandom(List<TaskSO> fromList, int count)
    {
        fromList = fromList.OrderBy(_ => rng.Next()).ToList();

        // Xáo trộn danh sách
        for (int i = 0; i < count && i < fromList.Count; i++)
        {
            DailyTasksProgress.Add(fromList[i].TaskType, 0);
            Debug.Log($"Added task: {fromList[i].TaskType} - {fromList[i].Description}");
        }
    }


    //Xử lý báo cáo tiến trình nhiệm vụ
    private void OnReportTaskProgress(object[] data)
    {
        TaskType taskType = (TaskType)data[0];
        int progress = (int)data[1];
        bool IsSpecialTile = (bool)data[2];

        // Đếm số ô đặc biệt đã sử dụng trong level
        if (IsSpecialTile) SpecialTileCount++;

        // Cập nhật tiến trình nhiệm vụ
        if (DailyTasksProgress.ContainsKey(taskType) && DailyTasksProgress[taskType] != -1) // Kiểm tra nếu nhiệm vụ chưa hoàn thành
        {
            // Nếu đã đạt mục tiêu, không cần cập nhật nữa
            if (DailyTasksProgress[taskType] == AllTasks[taskType].TargetAmount) return;

            // Cập nhật tiến trình
            DailyTasksProgress[taskType] += progress;
            GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);
            UIController.Instance.UpdateDailyMissionPanel();
        }
    }

   // Báo cáo tiến trình nhiệm vụ khi hoàn thành một level
    public void ReportProgressOnCompleteLevels()
    {
        // Báo cáo tiến trình nhiệm vụ khi hoàn thành level
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.SpendEnergy, 1, false});
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.CompleteLevels, 1, false});

        // Kiểm tra nhiệm vụ hoàn thành với 3 sao
        if (GameManager.Instance.LevelManager.CalculateStar() == 3)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.CompleteWith3Stars, 1, false});

        // Kiểm tra nhiệm vụ sử dụng ô đặc biệt
        if (SpecialTileCount == AllTasks[TaskType.UseMultipleSpecialTiles].TargetAmount)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.UseMultipleSpecialTiles, 1, false});

        // Kiểm tra nhiệm vụ không sử dụng ô đặc biệt
        if (SpecialTileCount == 0)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.NoSpecialTiles, 1, false});
        
        // Đặt lại bộ đếm ô đặc biệt
        SpecialTileCount = 0;
    }


    //Xử lý khi người chơi nhận nhiệm vụ hàng ngày
    private void OnClaimDailyTask(object[] data)
    {
        TaskType taskType = (TaskType)data[0];

        // Đánh dấu nhiệm vụ là đã hoàn thành
        if (DailyTasksProgress.ContainsKey(taskType))
        {
            DailyTasksProgress[taskType] = -1; // Đánh dấu là đã hoàn thành
            GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);
            UIController.Instance.UpdateDailyMissionPanel();

        }
        else
        {
            Debug.LogWarning($"Task {taskType} not found in daily tasks.");
        }

    }
    
}
