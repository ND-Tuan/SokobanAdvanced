using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSelectBox : MonoBehaviour
{
    [Header("Level Data")]
    public int LevelStage;
    public bool IsUnlocked;
    public int levelIndex;
    public int StarCount;


    [Header("UI Elements")]
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject UnlockPanel;
    [SerializeField] private GameObject[] stars;
    [SerializeField] private TextMeshProUGUI levelText;


    private void Start()
    {
        levelText.text = levelIndex.ToString();
        LevelStage = GetComponentInParent<StageInfo>().StageIndex;
        
        GameManager.Instance.LevelManager.RegisterLevelSelectBox(LevelStage, levelIndex, this);
    }

    private void OnEnable()
    {
        if(GameManager.Instance != null){
            SetLevelState();
        } else {
            // If GameManager is not initialized yet, wait for a bit and try again
            CancelInvoke(nameof(SetLevelState));
            Invoke(nameof(SetLevelState), 0.1f);
        }
    }

    private void SetLevelState()
    {
        StarCount = GameManager.Instance.LevelManager.GetLevelStatusValue(LevelStage, levelIndex);
        if (StarCount >= 0){
            SetLevelState(true);

            for (int i = 0; i < 3; i++){
                stars[i].SetActive(i < StarCount);
            }

        } else {
            SetLevelState(false);
        }
    }
    

    private void SetLevelState(bool isUnlocked)
    {
        IsUnlocked = isUnlocked;
        lockIcon.SetActive(!isUnlocked);
        UnlockPanel.SetActive(isUnlocked);


        //turn off all stars for sure
        stars[0].SetActive(false);
        stars[1].SetActive(false);
        stars[2].SetActive(false);
               
    }


    public void OnClick()
    {
        if(!GameManager.Instance.LevelManager.CheckEnergyAvailable()) return;
        
        if (IsUnlocked)
        {
            GameManager.Instance.LevelManager.LoadLevel(LevelStage, levelIndex);
        }
    }

}


