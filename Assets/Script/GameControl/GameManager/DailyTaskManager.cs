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
        Observer.AddListener(EvenID.ClaimDailyTask, OnClaimDailyTask);
        Observer.AddListener(EvenID.ReportTaskProgress, OnReportTaskProgress);
    }

    public async Task InitializeAsync()
    {
        await LoadTasks();
    }

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


    //=====Initialize daily tasks=====
    public void GetDailyTasks(DateTime now)
    {
        DailyTasksProgress.Clear();

        var playerData = GameManager.Instance.PlayerDataManager.PlayerData;

        // Reset daily tasks if it's a new day or if they haven't been set yet
        if ((playerData.DailyTaskID.Count == 0 && playerData.DailyTaskProgress.Count == 0)
            || now.Date != GameManager.Instance.PlayerDataManager.PlayerData.LastQuitTime.Date)
        {
            GetRandomTasks();
            return;
        }


        for (int i = 0; i < playerData.DailyTaskID.Count; i++)
        {
            TaskType taskType = (TaskType)playerData.DailyTaskID[i];
            int progress = playerData.DailyTaskProgress[i];

            DailyTasksProgress.Add(taskType, progress);
            UIController.Instance.UpdateDailyMissionPanel();

        }

    }

    //Randomize daily tasks
    private void GetRandomTasks()
    {
        List<TaskSO> easyTasks = AllTasks.Values.Where(t => t.PT == 20 && t.TaskType != TaskType.Login).ToList();
        List<TaskSO> mediumTasks = AllTasks.Values.Where(t => t.PT == 30).ToList();
        List<TaskSO> hardTasks = AllTasks.Values.Where(t => t.PT == 40).ToList();

        DailyTasksProgress.Add(TaskType.Login, 1); // Special case for CompleteLevels

        AddRandom(easyTasks, 2);
        AddRandom(mediumTasks, 2);
        AddRandom(hardTasks, 1);

        GameManager.Instance.PlayerDataManager.RestClaimed();
        GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);

        UIController.Instance.UpdateDailyMissionPanel();
    }

    private void AddRandom(List<TaskSO> fromList, int count)
    {
        fromList = fromList.OrderBy(_ => rng.Next()).ToList();
        // Shuffle the list
        for (int i = 0; i < count && i < fromList.Count; i++)
        {
            DailyTasksProgress.Add(fromList[i].TaskType, 0);
            Debug.Log($"Added task: {fromList[i].TaskType} - {fromList[i].Description}");
        }
    }


    //=====Handle task progress reporting=====
    private void OnReportTaskProgress(object[] data)
    {
        TaskType taskType = (TaskType)data[0];
        int progress = (int)data[1];
        bool IsSpecialTile = (bool)data[2];

        if (IsSpecialTile) SpecialTileCount++;

        if (DailyTasksProgress.ContainsKey(taskType) && DailyTasksProgress[taskType] != -1)
        {
            if (DailyTasksProgress[taskType] == AllTasks[taskType].TargetAmount) return;

            DailyTasksProgress[taskType] += progress;
            GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);
            UIController.Instance.UpdateDailyMissionPanel();
        }
    }

    //Report progress for tasks when completing a level
    public void ReportProgressOnCompleteLevels()
    {
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.SpendEnergy, 1, false});
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.CompleteLevels, 1, false});

        if (GameManager.Instance.LevelManager.CalculateStar() == 3)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.CompleteWith3Stars, 1, false});

        if (SpecialTileCount == AllTasks[TaskType.UseMultipleSpecialTiles].TargetAmount)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.UseMultipleSpecialTiles, 1, false});

        if (SpecialTileCount == 0)
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.NoSpecialTiles, 1, false});
            
        SpecialTileCount = 0;
    }


    //=====Handle when the player claims a daily task=====
    private void OnClaimDailyTask(object[] data)
    {
        TaskType taskType = (TaskType)data[0];

        if (DailyTasksProgress.ContainsKey(taskType))
        {
            DailyTasksProgress[taskType] = -1; // Mark as completed
            GameManager.Instance.PlayerDataManager.SaveDailyTaskData(DailyTasksProgress);
            UIController.Instance.UpdateDailyMissionPanel();

        }
        else
        {
            Debug.LogWarning($"Task {taskType} not found in daily tasks.");
        }

    }
    
}
