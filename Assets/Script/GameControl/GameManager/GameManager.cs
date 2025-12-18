using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ObserverPattern;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int NumberOfLevelsAvailable;
    [SerializeField] private int CoinDefaultAmount;
    [SerializeField] private int MaxEnergy;
    [SerializeField] private int EnergyRegenerationTime;
    public int EnergyCostToBuy { get; private set; } = 30;

    private float timer;
    private CancellationTokenSource saveLoopCTS;


    public PlayerDataManager PlayerDataManager { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public DailyTaskManager DailyTaskManager;

    //====Singleton================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);


            PlayerDataManager = new PlayerDataManager();
            LevelManager = new LevelManager();
            DailyTaskManager = new DailyTaskManager();

        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    private async void Start()
    {
        await PlayerDataManager.InitializeAsync(CoinDefaultAmount, MaxEnergy);
        await DailyTaskManager.InitializeAsync();
        await PlayerDataManager.LoadSkinDataAsync();

        await Task.Delay(100); // Đợi một chút để đảm bảo dữ liệu đã được tải

        DateTime ntpTime = await TimeManager.TryGetNetworkTimeUntilSuccess();

        UpdateEnergy(ntpTime);
        DailyTaskManager.GetDailyTasks(ntpTime);

        saveLoopCTS = new CancellationTokenSource();
        UIController.Instance.ShowAmountBar();
        await SaveTimeLoopAsync(saveLoopCTS.Token);

       
    }

    void Update()
    {
        RecoverEnergy();
    }


    //====Xử lý trạng thái game=========================
    public void GameWin()
    {
        //tính sao
        int star = LevelManager.CalculateStar();
        int starAmountDifference = LevelManager.UpdateLevelStatus(star);

        //trừ đi năng lượng
        if (PlayerDataManager.PlayerData.Energy == MaxEnergy) timer = 0;
        PlayerDataManager.SpendEnergy(1);

        //hiển thị ra ui 
        UIController.Instance.ShowLevelCompletePanel(star, starAmountDifference * 10);
        UIController.Instance.MinusEnergy();

        //mở khóa level tiếp theo
        LevelManager.UnlockNextLevel(NumberOfLevelsAvailable);

        //cập nhật dữ liệu vào file
        LevelManager.Save();
        PlayerDataManager.AddCoin(starAmountDifference * 10);

        //Báo cáo tiến trình nhiệm vụ
        DailyTaskManager.ReportProgressOnCompleteLevels();
    }

    public void GameOver()
    {
        UIController.Instance.ShowGameOverPanel();
    }

    //====Xử lý hồi năng lượng=========================

    //Cập nhật lại năng lượng khi vào game
    private void UpdateEnergy(DateTime now)
    {
        //Nếu năng lượng đã đầy thì không cần làm gì cả
        if (PlayerDataManager.PlayerData.Energy >= MaxEnergy) return;

        //tính toán thời gian đã trôi qua kể từ lần thoát game trước đó
        TimeSpan passed = now - PlayerDataManager.PlayerData.LastQuitTime;
        float savedTimer = PlayerDataManager.PlayerData.EnergyTimer;

        int secondsPassed = (int)(passed.TotalSeconds + savedTimer);

        int recovered = secondsPassed / EnergyRegenerationTime;
        int remainder = secondsPassed % EnergyRegenerationTime;

        recovered = Mathf.Min(recovered, MaxEnergy - PlayerDataManager.PlayerData.Energy);

        //Cập nhật lại vào PlayerData
        PlayerDataManager.AddEnergy(recovered);
        UIController.Instance.ShowAmountBar();

        timer = remainder;
    }

    public void RecoverEnergy()
    {
        //tính toán thời gian hồi năng lượng
        if (PlayerDataManager.PlayerData.Energy >= MaxEnergy) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= EnergyRegenerationTime)
        {
            int energyToAdd = (int)(timer / EnergyRegenerationTime);

            PlayerDataManager.AddEnergy(energyToAdd);

            timer %= EnergyRegenerationTime;

            //Cập nhật lên UI
            UIController.Instance.ShowAmountBar();
        }
    }

    public TimeSpan GetTimeToNextEnergy()
    {
        if (PlayerDataManager.PlayerData.Energy >= MaxEnergy)
            return TimeSpan.Zero;

        return TimeSpan.FromSeconds(EnergyRegenerationTime - timer);
    }

    public TimeSpan GetTimeToFullEnergy()
    {
        if (PlayerDataManager.PlayerData.Energy >= MaxEnergy)
            return TimeSpan.Zero;

        int energyMissing = MaxEnergy - PlayerDataManager.PlayerData.Energy;
        double totalSeconds = (energyMissing - 1) * EnergyRegenerationTime + (EnergyRegenerationTime - timer);
        return TimeSpan.FromSeconds(totalSeconds);
    }

    private async Task SaveTimeLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            DateTime ntpTime = await TimeManager.TryGetNetworkTimeUntilSuccess();

            //Lưu lại thời gian hiện tại
            PlayerDataManager.SaveEnergyTimer(ntpTime, timer);
            Debug.Log("Đã lưu thời gian: " + ntpTime.ToString("yyyy-MM-dd HH:mm:ss"));

            await Task.Delay(TimeSpan.FromSeconds(10), token);
        }
    }
    
    

}

public struct LevelID
{
    public int Stage;
    public int Index;
} 
