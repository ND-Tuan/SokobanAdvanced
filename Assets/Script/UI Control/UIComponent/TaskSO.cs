using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskData", menuName = "Tasks/Task")]
public class TaskSO : ScriptableObject
{
    [SerializeField] private TaskType taskType;
    [SerializeField] private int targetAmount = 1;
    [SerializeField] private string description;
    [SerializeField] private int pt = 20;

    public TaskType TaskType => taskType;
    public int TargetAmount => targetAmount;
    public string Description => description.Replace("#", targetAmount.ToString());
    public int PT => pt;
}

public enum TaskType {
    Login,
    UseNavigator,
    UseTeleporter,
    ActivateLever,
    UseObstacleMaker,
    UseFragileObstacle,
    CollectKey,
    CompleteWith3Stars,
    CompleteLevels,
    BuyEnergy,
    SpendEnergy,
    NoSpecialTiles,
    UseMultipleSpecialTiles
}
