using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ObserverPattern;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BasePanelController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Time")]
    [SerializeField] protected float AnimationTimeIn = 0.5f;
    [SerializeField] protected float AnimationTimeOut = 0.3f;
    [SerializeField] protected bool IsAnimation = false;
    [SerializeField] protected FxAudioDataSO panelShowFx;

    public virtual void Show()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, AnimationTimeIn*0.5f).SetUpdate(true).OnComplete(() => {
            canvasGroup.interactable = true;
        });
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
        canvasGroup.interactable = false;
        canvasGroup.DOFade(0, AnimationTimeOut*0.5f).SetUpdate(true);
    }

    //====UI Popup Animation========
    protected void Popup(float AnimationTime){
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;

        Observer.PostEvent(EvenID.PlayFX, panelShowFx);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(canvasGroup.DOFade(1, AnimationTime*0.75f).SetUpdate(true));
        seq.Join(transform.DOScale(1, AnimationTime).SetEase(Ease.OutBack));
        seq.OnComplete(() => canvasGroup.interactable = true);
    }

    protected void PopOut(float AnimationTime){
        canvasGroup.interactable = false;
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(canvasGroup.DOFade(0, AnimationTime));
        seq.Join(transform.DOScale(0, AnimationTime).SetEase(Ease.InBack));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    //====UI Panel Animation========
    protected void SlideAnimation(CanvasGroup panel,float animationTime, Vector3 startPos ,Vector3 targetPos, Ease easeType){
        panel.gameObject.SetActive(true);
        
        panel.alpha = 0;
        panel.transform.localPosition = startPos; 

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(panel.DOFade(1, animationTime*0.75f).SetUpdate(true));
        seq.Join(panel.gameObject.transform.DOLocalMove(targetPos, animationTime).From(startPos).SetEase(easeType));
    }

    protected void SlideOutAnimation(CanvasGroup panel, float animationTime, Vector3 targetPos, Vector3 startPos, Ease easeType){
        IsAnimation = true;
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(panel.DOFade(0, animationTime));
        seq.Join(panel.gameObject.transform.DOLocalMove(targetPos, animationTime).From(startPos).SetEase(easeType));
        seq.OnComplete(() => 
        {
            panel.gameObject.SetActive(false);
            IsAnimation = false;
        });
    }
}