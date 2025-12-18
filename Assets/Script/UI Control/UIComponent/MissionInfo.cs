using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using TMPro;
using UnityEngine;

public class MissionInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MissionDeltailText;
    [SerializeField] private TextMeshProUGUI MissionProgressText;
    [SerializeField] private TextMeshProUGUI MissionRewardText;

    private TaskType taskType;

    [SerializeField] private GameObject ClaimBTN;
    [SerializeField] private GameObject GoBTN;
    [SerializeField] private GameObject Claimed;


    public void SetMissionInfo(TaskSO task, int progress)
    {
        taskType = task.TaskType;
        MissionDeltailText.text = task.Description;
        MissionProgressText.text = $"{progress}/{task.TargetAmount}";
        MissionRewardText.text = $"{task.PT} pt";

        ClaimBTN.SetActive(task.TargetAmount == progress);
        GoBTN.SetActive(task.TargetAmount != progress);

        if(task.TargetAmount == progress)
        {
            Observer.PostEvent(EvenID.RedDdotMission, true);
        }
      
    }

    public void UpdateProgress(string progress)
    {
        MissionProgressText.text = progress;
    }

    public void SetClaimed()
    {
        ClaimBTN.SetActive(false);
        Claimed.SetActive(true);
        GoBTN.SetActive(false);
    }

    public void ClaimButton()
    {
        Observer.PostEvent(EvenID.RedDdotMission, false);
        Observer.PostEvent(EvenID.ClaimDailyTask, taskType);
        
    }

    public void GoButton()
    {
        if(taskType == TaskType.BuyEnergy)
        {
            
            return;
        }
        UIController.Instance.OnClickPlay();
    }
}
