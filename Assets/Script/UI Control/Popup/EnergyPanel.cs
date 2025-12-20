using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ObserverPattern;

public class EnergyPanel : BasePanelController
{
    [SerializeField] private TextMeshProUGUI CoolDownText;
    [SerializeField] private TextMeshProUGUI BuyAmountText;
    [SerializeField] private TextMeshProUGUI CostText;

    private TimeSpan NextEnergyTime;
    private TimeSpan FullEnergyTime;

    private int buyAmount;
    private int cost;
    
    // Update is called once per frame
    void Update()
    {
        // cập nhật thời gian hồi năng lượng
        NextEnergyTime = GameManager.Instance.GetTimeToNextEnergy();
        FullEnergyTime = GameManager.Instance.GetTimeToFullEnergy();

        // hiển thị thời gian lên UI
        if (FullEnergyTime.TotalSeconds > 0)
        {
            CoolDownText.text = "Recover in: " + NextEnergyTime.ToString(@"mm\:ss")
                                + "\nFull recover in: " + FullEnergyTime.ToString(@"hh\:mm\:ss");
        }
        else
        {
            CoolDownText.text = "Full Energy";
        }
    }

    public override void Show()
    {

        Popup(AnimationTimeIn);

        buyAmount = 1;
        cost = GameManager.Instance.EnergyCostToBuy;

        BuyAmountText.text = buyAmount.ToString();
        CostText.text = cost.ToString();
        Color color = GameManager.Instance.PlayerDataManager.CheckCoinEnough(cost) ? Color.white : Color.red;
        CostText.color = color;
    }

    public override void Hide()
    {
        PopOut(AnimationTimeOut);
    }

    public void AdjustBuyAmount(int amount)
    {
        // điều chỉnh số lượng năng lượng muốn mua
        if (buyAmount + amount < 1) return;

        buyAmount += amount;

        BuyAmountText.text = buyAmount.ToString();
        cost = GameManager.Instance.EnergyCostToBuy * buyAmount;
        CostText.text = cost.ToString();

        Color color = GameManager.Instance.PlayerDataManager.CheckCoinEnough(cost)? Color.white : Color.red;
        CostText.color = color;
    }

    //mua năng lượng
    public void BuyEnergy()
    {
        if (cost > GameManager.Instance.PlayerDataManager.PlayerData.Coin)
        {

            return;
        }

        //trừ xu và cộng năng lượng
        GameManager.Instance.PlayerDataManager.SpendCoin(cost);
        GameManager.Instance.PlayerDataManager.AddEnergy(buyAmount);

        UIController.Instance.ChangeCoinAmountEffect();
        UIController.Instance.ChangeEnergyAmountEffect();

        //báo cáo tiến độ nhiệm vụ mua năng lượng
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.BuyEnergy, 1, false});
    }
}
