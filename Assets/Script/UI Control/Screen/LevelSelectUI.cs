using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;
using ObserverPattern;

public class LevelSelectUI : BasePanelController
{
    [SerializeField] private AssetLabelReference StageUILabel;
    [SerializeField] private ScrollRect ScrollView;
    [SerializeField] private TextMeshProUGUI StageText;
    [SerializeField] private Image[] Components;
    private Dictionary<int, StageInfo> StageDic = new Dictionary<int, StageInfo>();
    private int currentStageIndex = 1;


    private void Start()
    {
        Addressables.LoadAssetsAsync<GameObject>(StageUILabel, (GameObject stage) =>
        {
            GameObject stageUI = Instantiate(stage, ScrollView.transform);
            StageInfo stageInfo = stageUI.GetComponent<StageInfo>();

            StageDic.Add(stageInfo.StageIndex, stageInfo);

            stageInfo.CanvasGroup.alpha = 0;
            stageUI.SetActive(false);
        });
    }

    public override void Show()
    {
        base.Show();
        StageDic[currentStageIndex].gameObject.SetActive(true);
        StageDic[currentStageIndex].CanvasGroup.alpha = 1;
        
        Observer.PostEvent(EvenID.ChangeMusic, StageDic[currentStageIndex].StageMainTheme);

        Vector3 position = StageDic[currentStageIndex].transform.localPosition;
        position.x = 0;
        StageDic[currentStageIndex].transform.localPosition = position;
        
        ScrollView.content = StageDic[currentStageIndex].GetComponent<RectTransform>();
        UIController.Instance.SetBackgroundImage(StageDic[currentStageIndex].BGColor);

         for (int i = 0; i < Components.Length; i++)
        {
            Components[i].DOColor(StageDic[currentStageIndex].MainColor, AnimationTimeIn).SetUpdate(true);
            
        }
    }


    private void ChangeStage(int stageIndex)
    {
        if(!StageDic.ContainsKey(stageIndex)) return;
       
        StageText.text = "Stage " + stageIndex.ToString();
        Observer.PostEvent(EvenID.ChangeMusic, StageDic[stageIndex].StageMainTheme);
        
        int x = stageIndex > currentStageIndex ? 1000 : -1000;

        ScrollView.content = StageDic[stageIndex].GetComponent<RectTransform>();

        SlideAnimation(StageDic[stageIndex].CanvasGroup, AnimationTimeIn, new Vector3(x, 0, 0), Vector3.zero, Ease.OutBack);
        SlideOutAnimation(StageDic[currentStageIndex].CanvasGroup, AnimationTimeOut, Vector3.zero, new Vector3(-x, 0, 0), Ease.InBack);

        for (int i = 0; i < Components.Length; i++)
        {
            Components[i].DOColor(StageDic[stageIndex].MainColor, AnimationTimeIn).SetUpdate(true);
            
        }

        UIController.Instance.SetBackgroundImage(StageDic[currentStageIndex].BGColor);
    }

    public void OnClickNextStage()
    {
        if(IsAnimation) return;
        if (currentStageIndex < StageDic.Count)
        {
            ChangeStage(currentStageIndex + 1);
            currentStageIndex++;
        }
    }
    public void OnClickPreviousStage()
    {
        if(IsAnimation) return;
        if (currentStageIndex > 1)
        {
            ChangeStage(currentStageIndex - 1);
            currentStageIndex--;
        }
    }

}
