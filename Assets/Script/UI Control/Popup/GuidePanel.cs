using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GuidePanel : BasePanelController
{
    [Header("Main Component")]
    [SerializeField] private CanvasGroup GuideFocus;
    [SerializeField] private GameObject Hand;
    [SerializeField] private TextMeshProUGUI GuideNameText;
    [SerializeField] private TextMeshProUGUI GuideDescriptionText;
    [SerializeField] private TextMeshProUGUI ButtonText;

    [Header("Load")]
    [SerializeField] private AssetLabelReference GuideDataLabel;
    private Dictionary<GuideType, GuideSO> guideSO = new();
    private List<GuideDisplayInfo> guideNeedToDisplayList = new();


    private void Awake()
    {
        Addressables.LoadAssetsAsync<GuideSO>(GuideDataLabel, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (GuideSO guide in handle.Result)
                {
                    guideSO.Add(guide.GuideType, guide);
                }
            }
            else
            {
                Debug.LogError("Failed to load GuideSO Addressables!");
            }

            gameObject.SetActive(false);
        };
    }

    public void ShowGuide(List<GuideDisplayInfo> guideTypes)
    {
        guideNeedToDisplayList = guideTypes;
        PanelSetUp(guideNeedToDisplayList[0]);
        
    }

    public void PanelSetUp(GuideDisplayInfo guideType)
    {
        if(!guideSO.ContainsKey(guideType.GuideType)) return;

        // Set the panel position
        AdjustPanelPosition(guideType.transform);

        // Set the display content
        // If there are more guides, show the next button
        ButtonText.text = guideNeedToDisplayList.Count > 1 ? "NEXT" : "CLOSE";

        // Set name and description
        GuideNameText.text = guideSO[guideType.GuideType].GuideName;
        GuideDescriptionText.text = guideSO[guideType.GuideType].GuideDescription;

        // Remove the guide that has been displayed
        guideNeedToDisplayList.Remove(guideType);

        Popup(AnimationTimeIn);

        // Show focus highlight
        GuideFocus.transform.position = guideType.transform.position + guideType.OffsetPos;
        GuideFocus.gameObject.SetActive(true);
        DisplayFocus();
    }

    // Adjust the position of the panel
    private void AdjustPanelPosition(Transform transform)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Calculate the new position of the panel based on the object's position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.transform.parent.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(transform.position),
            Camera.main,
            out Vector2 localPoint
        );

        // Change the pivot and panel direction based on the object's position
        if(localPoint.x >0) {
            rectTransform.pivot = new Vector2(1.1f, 1.15f);
            Hand.transform.localScale = new Vector3(-1, 1, 1);
            Hand.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, 200, 0);
            
        } else {
            rectTransform.pivot = new Vector2(-0.1f, 1.15f);
            Hand.transform.localScale = new Vector3(1, 1, 1);
            Hand.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, 200, 0);
        }

        // Change the position of the panel
        rectTransform.anchoredPosition = localPoint;
    }

    // Handle the logic for the next button
    public void NextGuide()
    {
        Debug.Log("Next Guide Clicked: " + guideNeedToDisplayList.Count + " remaining.");

        if(guideNeedToDisplayList.Count > 0){
            PanelSetUp(guideNeedToDisplayList[0]);

        } else {
            PopOut(AnimationTimeOut);
            HideFocus();
            Time.timeScale = 1;
        }
    }

    // Handle displaying the highlight panel
    private void DisplayFocus(){

        GuideFocus.alpha = 0;
        GuideFocus.transform.localScale = Vector3.one * 3;
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(GuideFocus.DOFade(1, AnimationTimeIn*0.75f).SetUpdate(true));
        seq.Join(GuideFocus.transform.DOScale(1, AnimationTimeIn*1.25f).SetEase(Ease.OutBack));
    }

    public void HideFocus()
    {
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(GuideFocus.DOFade(0, AnimationTimeOut));
        seq.Join(GuideFocus.transform.DOScale(0, AnimationTimeOut).SetEase(Ease.InBack));
        seq.OnComplete(() => GuideFocus.gameObject.SetActive(false));
    }
}



[System.Serializable]
public class GuideDisplayInfo
{
    public GuideType GuideType;
    public Transform transform;
    public Vector3 OffsetPos;
}
