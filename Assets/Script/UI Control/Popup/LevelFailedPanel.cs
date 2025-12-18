using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelFailedPanel : BasePanelController
{
    [Header("Main Components")]
    [SerializeField] private TextMeshProUGUI LevelText;

    public override void Show()
    {
        Popup(AnimationTimeIn);
        LevelID levelID = GameManager.Instance.LevelManager.CurrentLevel;
        LevelText.text = "Level " + levelID.Stage + "-" + levelID.Index;
    }

    public override void Hide()
    {
        PopOut(AnimationTimeOut);
    }
}
