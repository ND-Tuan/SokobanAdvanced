using UnityEngine;
using ObserverPattern;

public class ConveyorPressure : MonoBehaviour, IPowerRequire
{
    
    [SerializeField] private Diraction diraction;
    [Range(0, 3)]
    [SerializeField] private int NavigatorID;
    [SerializeField] private bool isPowered = true;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private GameObject EnergyRing;

    private bool originalPowerState;


    private void Awake()
    {
        originalPowerState = isPowered;
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.ActivateLever, 1, true});
        Invoke(nameof(ChangeDiraction), 0.1f);
            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Observer.PostEvent(EvenID.ChangeDiraction, new object[] {NavigatorID, null});
    }

    private void ChangeDiraction()
    {
        Observer.PostEvent(EvenID.ChangeDiraction, new object[] {NavigatorID, diraction});
    }

    public void ResetLevel()
    {
        SetPowerState(originalPowerState);
    }

    public void SetPowerState(bool state)
    {
        isPowered = state;
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);

        // Khi tắt nguồn, đảm bảo gửi tín hiệu dừng đổi hướng
        if (!isPowered)
        {
            Observer.PostEvent(EvenID.ChangeDiraction, new object[] {NavigatorID, null});
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        ColorChanger.ChangeColor(spriteRenderer, NavigatorID);
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);
    }
    #endif
  
}
