using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObserverPattern;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading.Tasks;

public class HomeUI : BasePanelController
{
    [SerializeField] private AudioClip HomeMainTheme;
    [SerializeField] private RectTransform playerIllu;
    [SerializeField] private Image playerSkin;
    [SerializeField] private GameObject missionRedDot;

    public override void Show()
    {
        base.Show();
        Observer.PostEvent(EvenID.ChangeMusic, HomeMainTheme);
        playerIllu.sizeDelta = new Vector2(150, 300);
        playerIllu.DOSizeDelta(new Vector2(150, 750), 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        UpdateSkin();
    }

    async void Start()
    {
        Observer.PostEvent(EvenID.ChangeMusic, HomeMainTheme);

        await WaitForSkinDataReady();

        UpdateSkin();
        Observer.AddListener(EvenID.RedDdotMission, OnDisplayRedDot);
    }

    private void UpdateSkin()
    {
        SkinSO CurrentSkin = GameManager.Instance.PlayerDataManager.GetEquippedSkin();
        if (CurrentSkin != null) playerSkin.sprite = CurrentSkin.SkinImage;
    }

    private async Task WaitForSkinDataReady()
    {
        // Chờ tới khi skinDataDictionary có dữ liệu
        while (GameManager.Instance.PlayerDataManager.SkinDataDictionary.Count == 0)
        {
            await Task.Yield(); // Chờ frame sau
        }
    }
    
    private void OnDisplayRedDot(object[] data)
    {
        bool isShow = (bool)data[0];
        missionRedDot.SetActive(isShow);
        
    }
}
