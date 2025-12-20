using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    private Tween CoinTween;
    
    [Header("Amount Bar")]
    [SerializeField] private GameObject AmountBar;
    [SerializeField] private TextMeshProUGUI CoinAmountText;
    [SerializeField] private TextMeshProUGUI EnergyAmountText;
    [SerializeField] private GameObject MinusEnergyEffect;

    [Header("Main Panels")]
    [SerializeField] private GameObject BlurPanel;

    [SerializeField] private HomeUI HomePanel;
    [SerializeField] private SkinSelectUI SkinSelectPanel;
    [SerializeField] private ShopUI ShopPanel;


    [Header("Popup Panel")]
    [SerializeField] private PauseSettingPanel SettingPanel;
    [SerializeField] private LevelCompletePanel levelCompletePanel;
    [SerializeField] private LevelFailedPanel levelFailedPanel;
    [SerializeField] private PauseSettingPanel PausePanel;
    [SerializeField] private EnergyPanel EnergyPanel;
    [SerializeField] private ConnectPanel ConnectPanel;
    [SerializeField] private DailyMissionPanel DailyMissionPanel;

    [Header("In Level Panel")]
    [SerializeField] private InPlayLevelUI InLevelUiPanel;
    [SerializeField] private GuidePanel GuidePanel;

    [Header("Level Select Panel")]
    [SerializeField] private LevelSelectUI LevelSelectPanel;
    [SerializeField] private SpriteRenderer BackgroundImage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //===Xử lý hiển thị ========
    public void ShowAmountBar()
    {
        AmountBar.SetActive(true);
        CoinAmountText.text = GameManager.Instance.PlayerDataManager.PlayerData.Coin.ToString();
        EnergyAmountText.text = GameManager.Instance.PlayerDataManager.PlayerData.Energy.ToString();
    }

    public void ChangeCoinAmountEffect()
    {
        int current = int.Parse(CoinAmountText.text);
        CoinTween = DOTween.To(() => current, x =>
        {
            current = x;
            CoinAmountText.text = current.ToString();

        }, GameManager.Instance.PlayerDataManager.PlayerData.Coin, 1f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
        {
            CoinAmountText.text = GameManager.Instance.PlayerDataManager.PlayerData.Coin.ToString();
        });
    }

    public void ChangeEnergyAmountEffect()
    {
        int current = int.Parse(EnergyAmountText.text);
        DOTween.To(() => current, x =>
        {
            current = x;
            EnergyAmountText.text = current.ToString();

        }, GameManager.Instance.PlayerDataManager.PlayerData.Energy, 1f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
        {
            EnergyAmountText.text = GameManager.Instance.PlayerDataManager.PlayerData.Energy.ToString();
        });
    }

    public async void MinusEnergy()
    {
        await Task.Delay(500);


        MinusEnergyEffect.SetActive(true);
        await Task.Delay(100);
        EnergyAmountText.text = GameManager.Instance.PlayerDataManager.PlayerData.Energy.ToString();

        await Task.Delay(400);
        MinusEnergyEffect.SetActive(false);
    }


    //doi mau backgroundImage
    public void SetBackgroundImage(Color color)
    {
        BackgroundImage.color = color;
    }

    public void ShowInPlayLevelUI()
    {
        Close();
        InLevelUiPanel.Show();
        AmountBar.SetActive(false);
    }

    public void SetMoveCountUI(int moveCount, int moveCountLimit, int[] moveToGetStar)
    {
        if(moveCount <= 0) return;
        InLevelUiPanel.SetMoveCount(moveCount, moveCountLimit);
        InLevelUiPanel.SetStarMarks(moveToGetStar, moveCountLimit);
    }

    public void DisplayGuide(List<GuideDisplayInfo> guideTypes)
    {
        GuidePanel.ShowGuide(guideTypes);
    }


    //===========================================
    public void ShowGameOverPanel()
    {
        Time.timeScale = 0;
        levelFailedPanel.Show();
        BlurPanel.SetActive(true);
    }


    //====== Xử lý hiển thị win panel========
    public void ShowLevelCompletePanel(int stars, int rewardAmount)
    {
        Time.timeScale = 0;
        BlurPanel.SetActive(true);
        levelCompletePanel.ShowPanel(stars, rewardAmount);
        AmountBar.SetActive(true);
    }


    //====== Xử lý hiển thị pause panel========
    public void ShowPausePanel()
    {
        Time.timeScale = 0;
        PausePanel.Show();
        BlurPanel.SetActive(true);
    }

    //====== Xử lý hiển thị connect panel========
    public void ShowConnecting()
    {
        Time.timeScale = 0;

        if(!ConnectPanel.gameObject.activeInHierarchy)
        {
            ConnectPanel.Show();
        }
        
        ConnectPanel.ShowConnecting();
        BlurPanel.SetActive(true);
    }
    

    public void ShowDisconnect()
    {
        Time.timeScale = 0;

        if(ConnectPanel == null) return;

        if (!ConnectPanel.gameObject.activeInHierarchy)
        {
            ConnectPanel.Show();
        }
       
        ConnectPanel.ShowDisconnect();
        BlurPanel.SetActive(true);
    }

    public void HideConnectPanel()
    {

        if(!ConnectPanel.gameObject.activeInHierarchy) return;
        Time.timeScale = 1;
        ConnectPanel.Hide();
        BlurPanel.SetActive(false);
    }

    //====== Xử lý hiển thị Daily Mission panel========
    public void ShowDailyMissionPanel()
    {
        Time.timeScale = 0;
        DailyMissionPanel.Show();
        BlurPanel.SetActive(true);
    }

    public void HideDailyMissionPanel()
    {
        Time.timeScale = 1;
        DailyMissionPanel.Hide();
        BlurPanel.SetActive(false);
    }

    public void UpdateDailyMissionPanel()
    {
        DailyMissionPanel.MissionsUiUpdate();
    }


    //====== Xử lý các button============
    public void OnClickResume()
    {
        Time.timeScale = 1;
        PausePanel.Hide();
        SettingPanel.Hide();
        BlurPanel.SetActive(false);
    }

    public void OnClickSetting()
    {
        Time.timeScale = 0;
        SettingPanel.Show();
        BlurPanel.SetActive(true);
    }

    public void OnClickHome()
    {
        Time.timeScale = 1;
        Close();
        HomePanel.Show();
        AmountBar.SetActive(true);
    }

    public void OnClickPlay()
    {
        Close();
        LevelSelectPanel.Show();
        AmountBar.SetActive(true);
    }

    public void OnClickBackToLevelSelect()
    {
        GameManager.Instance.LevelManager.UnloadLevel();
        GameManager.Instance.PlayerDataManager.SpendEnergy(1);
        Close();
        LevelSelectPanel.Show();
        AmountBar.SetActive(true);
    }

    public void NextStage()
    {
        LevelSelectPanel.OnClickNextStage();
    }

    public async void OnClickRestart()
    {
        if (!GameManager.Instance.LevelManager.CheckEnergyAvailable()) return;

        Close();
        await Task.Delay(100);
        GameManager.Instance.LevelManager.ReloadLevel();
    }

    public void OnClickNextLevel()
    {
        if (!GameManager.Instance.LevelManager.CheckEnergyAvailable()) return;

        Close();
        GameManager.Instance.LevelManager.LoadNextLevel();
    }

    public void OnClickShow(BasePanelController panel)
    {
        panel.Show();
    }

    public void OnClickCancel(BasePanelController panel)
    {
        panel.Hide();
    }

    private void Close()
    {
        Time.timeScale = 1;

        levelCompletePanel.Hide();
        levelFailedPanel.Hide();
        InLevelUiPanel.Hide();
        PausePanel.Hide();
        SettingPanel.Hide();
        LevelSelectPanel.Hide();
        EnergyPanel.Hide();
        DailyMissionPanel.Hide();

        BlurPanel.SetActive(false);
        HomePanel.Hide();
        SkinSelectPanel.Hide();
        ShopPanel.Hide();
        
    }
    
}
