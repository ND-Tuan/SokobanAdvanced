using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using DG.Tweening;
using ObserverPattern;

public class LevelCompletePanel : BasePanelController
{    
    [Header("Main Components")]
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private GameObject[] Stars;
    [SerializeField] private TextMeshProUGUI RewardText;
    [SerializeField] private FxAudioDataSO StarPopup;
    [SerializeField] private GameObject Reward;
    [SerializeField] private RectTransform CoinBar;
    [SerializeField] private GameObject RewardCoin;


    [Header("Settings")]
    [SerializeField] private float starPopupDelay = 0.5f;       // Delay between each star
    [SerializeField] private int maxCoinsToSpawn = 10;           // Limit for flying coins


    private Tween rewardTween;

    public async void ShowPanel(int star, int rewardAmount)
    {
        Time.timeScale = 0;
        
        // Turn off all stars
        for (int i = 0; i < Stars.Length; i++)
        {
            Stars[i].SetActive(false);
        }

        Popup(AnimationTimeIn);
        ShowRewardAnimated(rewardAmount);

        LevelID levelID = GameManager.Instance.LevelManager.CurrentLevel;
        LevelText.text = "Level " + levelID.Stage + "-" + levelID.Index;

        await Task.Delay((int)(AnimationTimeIn * 900));

        // Show reward text

        for (int i = 0; i < star && i < Stars.Length; i++)
        {
            Stars[i].SetActive(true);
            Observer.PostEvent(EvenID.PlayFX, StarPopup);
            await Task.Delay((int)(starPopupDelay * 1000));
        }
    }

    public override void Hide()
    {
        PopOut(AnimationTimeOut);
    }

    private void ShowRewardAnimated(int rewardAmount){
        Reward.SetActive(!(rewardAmount == 0));

        if (rewardAmount == 0) return;

        int current = 0;
        rewardTween = DOTween.To(() => current, x => {
            current = x;
            RewardText.text = "+" + current;

        }, rewardAmount, 1f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => {
            SpawnRewardCoins(rewardAmount);
        });
    }

    private async void SpawnRewardCoins(int rewardAmount)
    {
        rewardTween?.Kill();

        await Task.Delay(100); // Wait 0.1 seconds before starting to fly coins
        
        int spawnCount = Mathf.Min(rewardAmount, maxCoinsToSpawn);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject coin = PoolManager.Instance.Get(RewardCoin, RewardText.transform.position);
            if (coin.TryGetComponent(out CoinFly coinFly))
            {
                coinFly.target = CoinBar; // Fly towards CoinBar
            }
            
            await Task.Delay(50);
        }

        
        await Task.Delay(100);
        UIController.Instance.ChangeCoinAmountEffect(); // Update the coin amount in the UI
    }

}
