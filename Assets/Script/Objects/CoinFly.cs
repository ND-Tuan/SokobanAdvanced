using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class CoinFly : MonoBehaviour
{
    public Transform target;

    [Header("Movement Settings")]
    [SerializeField] private float scatterRadius = 150f;
    [SerializeField] private float scatterDuration = 0.3f;
    [SerializeField] private float delayBeforeFly = 0.2f;
    [SerializeField] private float flyDuration = 0.8f;
    [SerializeField] private Ease flyEase = Ease.InOutSine;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 startScale = Vector3.one;
    [SerializeField] private Vector3 endScale = new Vector3(0.3f, 0.3f, 0.3f);

    private void OnEnable()
    {
        ResetCoin();
        StartScatter();
    }

    private void OnDisable()
    {
        // Kill all tweens associated with this transform to avoid errors
        transform.DOKill();
    }

    private void ResetCoin()
    {
        transform.localScale = startScale;
    }

    // Bắt đầu hiệu ứng scatter
    private void StartScatter()
    {

        Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
        Vector3 scatterTargetPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(transform.DOMove(scatterTargetPos, scatterDuration).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(startScale*1.2f, scatterDuration).SetEase(Ease.OutCubic));
        seq.OnComplete(() =>{
            if (!gameObject.activeSelf) return;
            StartFlyToTarget();
        });
    }

    // Bắt đầu hiệu ứng bay về mục tiêu
    private async void StartFlyToTarget()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        await Task.Delay((int)(delayBeforeFly * 1000));

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); 
        seq.Append(transform.DOMove(target.position, flyDuration).SetEase(flyEase));
        seq.Join(transform.DOScale(endScale, flyDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() =>{
            gameObject.SetActive(false);
            
        });
    }
}
