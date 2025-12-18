using UnityEngine;
using ObserverPattern;

public class DoorPressure : MonoBehaviour, IPowerRequire
{
    
    [SerializeField] private bool isPowered = true;
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
        Invoke(nameof(changeState), 0.1f);
            
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Observer.PostEvent(EvenID.ChangeDoorState, new object[] { false });
    }

    private void changeState()
    {
        Observer.PostEvent(EvenID.ChangeDoorState, new object[] { true });
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
        
        // Khi tắt nguồn, đảm bảo gửi tín hiệu đóng cửa
        if (!isPowered)
        {
            Observer.PostEvent(EvenID.ChangeDoorState, new object[] { false });
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);
    }
    #endif
  
}

