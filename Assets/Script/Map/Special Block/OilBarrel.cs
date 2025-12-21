using System.Collections;
using ObserverPattern;
using UnityEngine;

public class OilBarrel : MonoBehaviour, IResetLevel
{
    [SerializeField] private GameObject Barrel;
    [SerializeField] private Animator ExplosionEffect;
    [SerializeField] private Animator ExplosionEffect2;
    [SerializeField] private GameObject Mark;
    [SerializeField] private float ActiveDistance = 2f;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private FxAudioDataSO ExplosionAudioData;
    [SerializeField] private LayerMask BoxLayer; // Thêm layer mask để tối ưu
    public bool isExploded = false;
    private float checkInterval = 0.1f; // Giảm tần suất kiểm tra
    private float nextCheckTime = 0f;

    void FixedUpdate()
    {
        if (Time.time >= nextCheckTime)
        {
            CheckFire();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void CheckFire() 
    {
        if(isExploded) return;
        // Tìm tất cả MagnetBox gần đây
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, ActiveDistance, BoxLayer);

        foreach (Collider2D col in nearbyColliders)
        {   
            if (!col.CompareTag("FireBox")) continue;
                
            float distance = Vector2.Distance(transform.position, col.gameObject.transform.position);
            if (distance <= ActiveDistance && IsAdjacent(col.gameObject))
            {
                // Kích hoạt thùng dầu
                Explode(0);
                Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.BlowUpOilBarrel, 1, true});
            }
        }
    }

    public void Explode( float delay )
    {
        StopAllCoroutines();
        StartCoroutine(TriggerExplosion(delay));
    }


    /// Kích hoạt vụ nổ sau một khoảng thời gian trì hoãn
    IEnumerator TriggerExplosion(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Bắt đầu hiệu ứng nổ
        isExploded = true;
        Barrel.SetActive(false);
        ExplosionEffect.Play("Explosion", 0, 0f);
        ExplosionEffect2.Play("Blink", 0, 0f);

        // Kích hoạt các thùng dầu liền kề
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, ActiveDistance);
        foreach (Collider2D col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue;

            if(col.gameObject.TryGetComponent<OilBarrel>(out OilBarrel otherBarrel))
            {
                float dis = Vector2.Distance(transform.position, otherBarrel.transform.position);
                if (dis <= ActiveDistance && IsAdjacent(otherBarrel.gameObject) && !otherBarrel.isExploded)
                {
                    // Kích hoạt thùng dầu
                    otherBarrel.Explode(0.5f);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        Observer.PostEvent(EvenID.PlayFX, ExplosionAudioData);

        yield return new WaitForSeconds(0.5f);
        boxCollider2D.enabled = false;
        Mark.SetActive(true);
    }


    // Kiểm tra xem hai đối tượng có liền kề nhau không
    private bool IsAdjacent(GameObject other)
    {
        Vector2 diff = other.transform.position - transform.position;
        float absDiffX = Mathf.Abs(diff.x);
        float absDiffY = Mathf.Abs(diff.y);

        bool horizontallyAdjacent = (absDiffX >= 0.9f && absDiffX <= 1.1f) && absDiffY < 0.2f;
        bool verticallyAdjacent = (absDiffY >= 0.9f && absDiffY <= 1.1f) && absDiffX < 0.2f;

        return horizontallyAdjacent || verticallyAdjacent;
    }

    public void ResetLevel()
    {
        Barrel.SetActive(true);
        Mark.SetActive(false);
        boxCollider2D.enabled = true;
        isExploded = false;
    }
}
