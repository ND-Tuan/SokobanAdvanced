using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObserverPattern;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class SkinSelectUI : BasePanelController
{
    [Header("Main Components")]
    [SerializeField] private TextMeshProUGUI SkinName;
    [SerializeField] private TextMeshProUGUI SkinPrice;
    [SerializeField] private GameObject MoveEffectIcon;

    [SerializeField] private GameObject PriceBoard;
    [SerializeField] private GameObject CoinIcon;
    [SerializeField] private GameObject BuyButton;
    [SerializeField] private GameObject EquipButton;
    [SerializeField] private GameObject Equipped;

    [SerializeField] private RectTransform playerIllu;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject SkinPlaceholder;


    public float[] pos;
    private float distance;
    private float scroll_pos;
    private int currentIndex = -1;

    private bool isSetup = false;

    private async void Start()
    {
        await WaitForSkinDataReady();

        //setup cho content
        ContentSetUp();

        //padding tự động cho content
        SetAutoPadding();
    }

    private async Task WaitForSkinDataReady()
    {
        // Chờ tới khi skinDataDictionary có dữ liệu
        while (GameManager.Instance.PlayerDataManager.SkinDataDictionary.Count == 0)
        {
            await Task.Yield(); // Chờ frame sau
        }
    }

    private void Update()
    {
        if (!isSetup) return;
        
        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.value;
        }
        else
        {
            int nearestIndex = GetNearestIndex(scroll_pos);
            scrollbar.value = Mathf.Lerp(scrollbar.value, pos[nearestIndex], 0.1f);
        }

        int newIndex = GetNearestIndex(scroll_pos);
        if (newIndex != currentIndex)
        {
            UpdateSlideScales(newIndex);
            UpdateUI(newIndex);

            currentIndex = newIndex;
        }
    }


    public override void Show()
    {
        base.Show();
        playerIllu.sizeDelta = new Vector2(playerIllu.sizeDelta.x, 750);
        playerIllu.DOSizeDelta(new Vector2(playerIllu.sizeDelta.x, 850), 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        
    }

    private int GetNearestIndex(float value)
    {
        float min = Mathf.Abs(value - pos[0]);
        int index = 0;

        for (int i = 1; i < pos.Length; i++)
        {
            float dist = Mathf.Abs(value - pos[i]);
            if (dist < min)
            {
                min = dist;
                index = i;
            }
        }

        return index;
    }

    void UpdateSlideScales(int focusIndex)
    {
        for (int i = 0; i < pos.Length; i++)
        {
            Transform slide = content.transform.GetChild(i);
            Vector3 targetScale = (i == focusIndex) ? new Vector3(1f, 1f, 1f) : new Vector3(0.8f, 0.8f, 1f);

            // Dừng tween cũ nếu có
            slide.DOKill();
            // Tween đến scale mới
            slide.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    //cập nhật lại ui theo skin được chọn 
    private void UpdateUI(int index)
    {
        // Cập nhật tên và giá của skin
        SkinName.text = GameManager.Instance.PlayerDataManager.SkinDataDictionary[index].SkinName;

        MoveEffectIcon.SetActive(GameManager.Instance.PlayerDataManager.SkinDataDictionary[index].SpecialEffect);

        int price = GameManager.Instance.PlayerDataManager.SkinDataDictionary[index].Price;

        Color color = GameManager.Instance.PlayerDataManager.CheckCoinEnough(price) ? Color.white : Color.red;
        SkinPrice.color = color;

        if (price > 0)
        {
            SkinPrice.text = price.ToString();
            CoinIcon.SetActive(true);
            
        }
        else
        {
            SkinPrice.text = "Free";
            CoinIcon.SetActive(false);
        }

        //Trạng thái skin
        bool isBuy = GameManager.Instance.PlayerDataManager.CheckSkinOwned(index);

        Equipped.SetActive(false);
        BuyButton.SetActive(!isBuy);
        EquipButton.SetActive(isBuy);
        PriceBoard.SetActive(!isBuy);


        if (index == GameManager.Instance.PlayerDataManager.PlayerData.SkinEquipped)
        {
            EquipButton.SetActive(false);
            Equipped.SetActive(true);
        }

    }

    

    //
    private void ContentSetUp()
    {
        Dictionary<int, SkinSO> skinList = GameManager.Instance.PlayerDataManager.SkinDataDictionary;

        for (int i = 0; i < skinList.Count; i++)
        {
            SkinSO skin = skinList[i];
            GameObject skinItem = Instantiate(SkinPlaceholder, content);
            skinItem.GetComponent<Image>().sprite = skin.SkinImage;
        }

        // Set giá trị scrollbar ban đầu
        int childCount = skinList.Count;
        pos = new float[childCount];
        distance = 1f / (childCount - 1);

        for (int i = 0; i < childCount; i++)
        {
            pos[i] = distance * i;
        }

        isSetup = true;
    }

    // Thiết lập padding tự động cho content để căn giữa item
    private void SetAutoPadding()
    {
        float viewportWidth = viewport.rect.width;
        float itemWidth = content.GetChild(0).GetComponent<RectTransform>().rect.width;

        // Tính padding để căn giữa
        int sidePadding = Mathf.RoundToInt((viewportWidth - itemWidth) / 2f);

        content.GetComponent<HorizontalLayoutGroup>().padding.left = sidePadding;
        content.GetComponent<HorizontalLayoutGroup>().padding.right = sidePadding;

        // Cập nhật lại layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    //Xử lý sự kiện khi người dùng click vào nút mua skin
    public void OnClickBuyButton()
    {
        int skinID = currentIndex;
        SkinSO skin = GameManager.Instance.PlayerDataManager.SkinDataDictionary[skinID];

        if (GameManager.Instance.PlayerDataManager.CheckCoinEnough(skin.Price))
        {
            GameManager.Instance.PlayerDataManager.BuySkin(skinID);

            // Cập nhật lại UI
            UpdateUI(currentIndex);
            UIController.Instance.ChangeCoinAmountEffect();
        }
        else
        {
            Debug.Log("Not enough coins");
        }
    }
    
    public void OnClickEquipButton()
    {
        int skinID = currentIndex;
        GameManager.Instance.PlayerDataManager.EquipSkin(skinID);

        // Cập nhật lại UI
        UpdateUI(currentIndex);
    }

}
