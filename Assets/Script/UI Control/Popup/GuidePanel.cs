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

        //điều chỉnh vị trí panel
        AdjustPanelPosition(guideType.transform);

        //Đổi text nút
        //nếu còn nhiều hướng dẫn thì hiện nút NEXT, nếu chỉ còn hướng dẫn cuối thì hiện nút CLOSE
        ButtonText.text = guideNeedToDisplayList.Count > 1 ? "NEXT" : "CLOSE";

        //Đổi tên và mô tả hướng dẫn
        GuideNameText.text = guideSO[guideType.GuideType].GuideName;
        GuideDescriptionText.text = guideSO[guideType.GuideType].GuideDescription;

        // Loại bỏ hướng dẫn đã được hiển thị
        guideNeedToDisplayList.Remove(guideType);

        Popup(AnimationTimeIn);

        // Hiển thị vùng highlight
        GuideFocus.transform.position = guideType.transform.position + guideType.OffsetPos;
        GuideFocus.gameObject.SetActive(true);
        DisplayFocus();
    }

    // Điều chỉnh vị trí của bảng hướng dẫn dựa trên vị trí của đối tượng được truyền vào
    private void AdjustPanelPosition(Transform transform)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Tính toán vị trí mới của bảng dựa trên vị trí của đối tượng
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.transform.parent.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(transform.position),
            Camera.main,
            out Vector2 localPoint
        );

        // Thay đổi điểm neo và hướng của bảng dựa trên vị trí của đối tượng
        if(localPoint.x >0) {
            rectTransform.pivot = new Vector2(1.1f, 1.15f);
            Hand.transform.localScale = new Vector3(-1, 1, 1);
            Hand.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, 200, 0);
            
        } else {
            rectTransform.pivot = new Vector2(-0.1f, 1.15f);
            Hand.transform.localScale = new Vector3(1, 1, 1);
            Hand.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, 200, 0);
        }

        // Thay đổi vị trí của bảng
        rectTransform.anchoredPosition = localPoint;
    }

    // Xử lý logic cho nút tiếp theo
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

    // Hiển thị hiệu ứng vùng highlight
    private void DisplayFocus(){

        GuideFocus.alpha = 0;
        GuideFocus.transform.localScale = Vector3.one * 3;
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(GuideFocus.DOFade(1, AnimationTimeIn*0.75f).SetUpdate(true));
        seq.Join(GuideFocus.transform.DOScale(1, AnimationTimeIn*1.25f).SetEase(Ease.OutBack));
    }

    // Ẩn hiệu ứng vùng highlight
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
