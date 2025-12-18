using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InPlayLevelUI : BasePanelController
{
    [Header("Main Components")]
    [SerializeField] private CanvasGroup HeaderPanel;
    [SerializeField] private Vector3 HeaderPanelStartPos;
    [SerializeField] private Vector3 HeaderPanelTargetPos;

    [Header("Move Count UI")]
    [SerializeField] private TextMeshProUGUI MoveCountText;
    [SerializeField] private Slider MoveCountSlider;
    [SerializeField] private Slider[] StarMarks;


    public override void Show()
    {
        base.Show();
        SlideAnimation(HeaderPanel, AnimationTimeIn, HeaderPanelStartPos, HeaderPanelTargetPos, Ease.OutBack);
    }

    public void SetMoveCount(int moveCount, int moveCountLimit)
    {
        MoveCountText.text = moveCount.ToString();
        MoveCountSlider.DOValue((float)moveCount / moveCountLimit, 0.5f).SetEase(Ease.Linear).SetUpdate(true);
    }

    public void SetStarMarks(int[] moveToGetStar, int maxMoves)
    {
        for (int i = 0; i < 3; i++)
        {
            float value = (moveToGetStar[i]-1) / (float)maxMoves;
            StarMarks[i].value = value;
        }
    }
}
