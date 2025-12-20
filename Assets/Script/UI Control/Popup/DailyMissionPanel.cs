using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ObserverPattern;

public class DailyMissionPanel : BasePanelController
{
    [SerializeField] private Slider PTSlider;
    [SerializeField] private TextMeshProUGUI PTText;
    [SerializeField] private MissionInfo[] MissionInfos;

    [SerializeField] private GameObject[] RewardBTN;
    [SerializeField] private GameObject[] DoneIcons;
    [SerializeField] private GameObject[] ClaimableIcons;

    private int currentPT;

    // cập nhật giao diện nhiệm vụ hàng ngày
    public void MissionsUiUpdate()
    {
        int totalPT = 0;

        // reset trạng thái thông báo
        Observer.PostEvent(EvenID.RedDdotMission, false);

        int i = 0;
        // duyệt qua tất cả nhiệm vụ và cập nhật giao diện
        foreach (var task in GameManager.Instance.DailyTaskManager.DailyTasksProgress)
        {
            if (i >= MissionInfos.Length)
                break;
    
            if (!GameManager.Instance.DailyTaskManager.AllTasks.TryGetValue(task.Key, out TaskSO taskSO))
            {
                Debug.LogError($"TaskSO not found for TaskType: {task.Key}");
                continue;
            }
            
            if (task.Value == -1)
            {
                // nhiệm vụ đã hoàn thành và đã nhận thưởng
                MissionInfos[i].gameObject.transform.SetAsLastSibling();
                MissionInfos[i].SetClaimed();
                totalPT += taskSO.PT;
                i++;

                continue;
            }

            // tính tổng điểm nhiệm vụ đã hoàn thành
            MissionInfos[i].SetMissionInfo(taskSO, task.Value);

            if(task.Value >= taskSO.TargetAmount)
            {
               MissionInfos[i].gameObject.transform.SetAsFirstSibling();
            }
           
            i++;
        }
        currentPT = totalPT;

        PTSlider.DOValue((float)totalPT / 100, 0.5f).SetEase(Ease.Linear).SetUpdate(true);
        PTText.text = $"{totalPT}";
        UpdateRewardButtons();
    }
    
    // cập nhật trạng thái nút nhận thưởng
    public void UpdateRewardButtons()
    {   
        // reset trạng thái thông báo
        int DailyClaimed = GameManager.Instance.PlayerDataManager.PlayerData.DailyClaimed;

        // duyệt qua các nút nhận thưởng và cập nhật trạng thái
        for (int i = 0; i < RewardBTN.Length; i++)
        {
            RewardBTN[i].SetActive(false);
            DoneIcons[i].SetActive(i < DailyClaimed);
            ClaimableIcons[i].SetActive(false);
        }

        // kiểm tra điều kiện để hiển thị nút nhận thưởng
        if (currentPT >= 20 && !DoneIcons[0].activeInHierarchy && DailyClaimed < 1)
        {
            RewardBTN[0].SetActive(true);
            ClaimableIcons[0].SetActive(true);
            Observer.PostEvent(EvenID.RedDdotMission, true);
        }

        if (currentPT >= 50 && !DoneIcons[1].activeInHierarchy && DailyClaimed < 2)
        {
            RewardBTN[1].SetActive(true);
            ClaimableIcons[1].SetActive(true);
            Observer.PostEvent(EvenID.RedDdotMission, true);
        }

        if (currentPT >= 100 && !DoneIcons[2].activeInHierarchy && DailyClaimed < 3)
        {
            RewardBTN[2].SetActive(true);
            ClaimableIcons[2].SetActive(true);
            Observer.PostEvent(EvenID.RedDdotMission, true);
        }
    }

    // xử lý khi nhấn nút nhận thưởng
    public void OnClickClaimReward(int index)
    {
        // cập nhật trạng thái đã nhận thưởng
        switch (index)
        {
            case 0:
                GameManager.Instance.PlayerDataManager.ClaimReward();
                GameManager.Instance.PlayerDataManager.AddCoin(20);

                break;
            case 1:
                GameManager.Instance.PlayerDataManager.ClaimReward();
                GameManager.Instance.PlayerDataManager.AddCoin(20);
                break;
            case 2:
                GameManager.Instance.PlayerDataManager.ClaimReward();
                GameManager.Instance.PlayerDataManager.AddCoin(40);
                GameManager.Instance.PlayerDataManager.AddEnergy(5);
                break;
            default:
                Debug.LogError("Invalid reward index");
                return;
        }

        // cập nhật giao diện sau khi nhận thưởng
        UIController.Instance.ChangeCoinAmountEffect();
        UIController.Instance.ChangeEnergyAmountEffect();

        DoneIcons[index].SetActive(true);
        ClaimableIcons[index].SetActive(false);
        RewardBTN[index].SetActive(false);

        Observer.PostEvent(EvenID.RedDdotMission, false);
        UpdateRewardButtons();
    }

    public override void Show()
    {
        Popup(AnimationTimeIn);

    }

    public override void Hide()
    {
        PopOut(AnimationTimeOut);
    }
    


}
