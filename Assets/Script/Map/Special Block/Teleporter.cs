using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ObserverPattern;
using System;

public class Teleporter : MonoBehaviour, IPowerRequire
{
    private ParticleSystem[] teleportParticles;
    [Range(0, 5)]
    [SerializeField] private int TeleporterID;
    [SerializeField] private bool isTeleporting = false;
    [SerializeField] private bool isPowered = true;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private GameObject EnergyRing;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private FxAudioDataSO TeleportAudioData;

    void  Awake()
    {
        Observer.AddListener(EvenID.Teleport, TeleportObject);
    }

    //Khi có vật thể đi vào teleporter
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTeleporting)
        {
            isTeleporting = true;
            Vector3 direction = transform.position - other.transform.position;

            //Report task progress
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.UseTeleporter,  1, true});

            //Bắt đầu dịch chuyển vật thể
            StartCoroutine(WaitAndTeleport(other, direction));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        
        isTeleporting = false;
        
    }

    //Xử lý dịch chuyển vật thể
    private void TeleportObject(object[] data)
    {
        if (!CheckTeleportable())
        {
            isTeleporting = false;
            return;
        } 

        isTeleporting = true;
        int teleporterID = (int)data[0];
        Collider2D other = (Collider2D)data[1];
        GameObject teleporter = (GameObject)data[2];
        Vector2 direction = (Vector2)data[3];

        direction = To4Direction(direction);

        // Kiểm tra va chạm với chướng ngại vật ở phía trước
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + direction*0.5f, direction, 0.8f, obstacleLayer);

        if (hit.collider != null) direction = Vector2.zero;
       
        if (teleporterID == TeleporterID && teleporter != this.gameObject)
        {
            isTeleporting = true;

            //await Task.Delay(10); // Simulate teleportation delay
            //Tính hướng di chuyển sau khi dịch chuyển
          
            Debug.Log(direction +"," + gameObject);
           
            IMoveable Object = other.GetComponent<IMoveable>();

            if (Object != null)
            {
                Object.ChangeDirection(direction, transform.position);
            }

            StartCoroutine(ResetTeleportingState());
        }
    }

    IEnumerator ResetTeleportingState()
    {
        yield return new WaitForSeconds(0.1f);

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        if (nearbyColliders.Length <= 1)
            isTeleporting = false;
    }

    private bool CheckTeleportable()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        
        return nearbyColliders.Length <= 1;
    }
    

    //Đợi đến khi người chơi vào đúng vị trí rồi dịch chuyển
    IEnumerator WaitAndTeleport(Collider2D other, Vector2 direction)
    {
        yield return new WaitUntil(() => Vector2.Distance(other.transform.position, transform.position) < 0.1f);
        
        //Gửi sự kiện dịch chuyển
        Observer.PostEvent(EvenID.Teleport, new object[] {TeleporterID, other, this.gameObject, direction });

        //Chạy âm thanh hiệu ứng dịch chuyển ở đây
        Observer.PostEvent(EvenID.PlayFX, TeleportAudioData);
    }

    private void OnDestroy()
    {
        Observer.RemoveListener(EvenID.Teleport, TeleportObject);
    }

    private void OnValidate()
    {
       ChangeParticleColor(ColorChanger.GetColorByID(TeleporterID) );
       SetPowerState(isPowered);
    }

    //Thay đổi màu hiệu ứng theo màu của teleporter
    private void ChangeParticleColor(Color color)
    {
        teleportParticles = GetComponentsInChildren<ParticleSystem>();

        ParticleSystem.MainModule mainModule;
        foreach (ParticleSystem particle in teleportParticles)
        {
            mainModule = particle.main;
            mainModule.startColor = color;
        }
    }


    //Chuyển hướng về 4 hướng chính
    Vector2 To4Direction(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? Vector2.right : Vector2.left;
        else
            return dir.y > 0 ? Vector2.up : Vector2.down;
    }

    public void SetPowerState(bool state)
    {
        isPowered = state;
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);
        ChangeParticleColor(isPowered? ColorChanger.GetColorByID(TeleporterID) : new Color(0,0,0,0));

    }
}
