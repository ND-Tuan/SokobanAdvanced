using System.Collections.Generic;
using UnityEngine;
using ObserverPattern;

public class Door : MonoBehaviour, IResetLevel
{
    
    [SerializeField] private Animator animator; 
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private float adjacentSearchRadius = 1.5f; // Bán kính tìm kiếm cửa liền kề
    [SerializeField] private List<Door> adjacentDoors = new List<Door>(); 
    
    [SerializeField] private LayerMask blockingObjectsLayer; 
    [SerializeField] private Vector2 detectionSize = new Vector2(0.9f, 0.9f); 
    
    // Biến trạng thái
    private bool isOpen = false; 
    private bool isBlocked = false; 
    private bool isActived = false;


    private void Awake()
    {
        Observer.AddListener(EvenID.ChangeDoorState, OnChangeDoorState);
    }
    
    private void Start()
    {
        // Khởi tạo cửa ở trạng thái đóng
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();
        
        FindAdjacentDoors();
        
    }

    /// Kiểm tra liên tục xem cửa có bị chặn không
    private void Update()
    {
        // Cập nhật trạng thái isBlocked liên tục
        isBlocked = IsBlocked();
        
        // Nếu cửa đang mở và không còn bị chặn, thử đóng cửa
        if (isOpen && !isBlocked && !isActived)
        {
            CloseDoor();
        }
    }
    
    public void OpenDoor()
    {
        if (!isOpen){

            isOpen = true;
            Debug.Log("Opening door at " + transform.position);

            animator.SetBool("Open", true);
            doorCollider.enabled = false;

            // Mở các cửa liền kề
            foreach (Door door in adjacentDoors)
            {
                door.OpenDoor();
            }
            
        }
           
    }
    
    public void CloseDoor()
    {
        if (isOpen && !IsBlocked()){
            isOpen = false;
            animator.SetBool("Open", false);
            doorCollider.enabled = true;

            // Đóng các cửa liền kề
            foreach (Door door in adjacentDoors)
            {
                door.CloseDoor();
            }
        }

        if (IsBlocked())
        {
            Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.BlockDoor, 1, false});
        }
            
    }
    
    /// Xử lý sự kiện thay đổi trạng thái cửa
    private void OnChangeDoorState(object[] data)
    {
        isActived = (bool)data[0];

        if (isActived)
            OpenDoor();
        else
            CloseDoor();
    }
    
    
    /// Tự động tìm các cửa liền kề trong bán kính xác định
    private void FindAdjacentDoors()
    {
        // Xóa danh sách cũ
        adjacentDoors.Clear();     

        // Tìm tất cả các cửa trong bán kính
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, adjacentSearchRadius);
        foreach (Collider2D col in nearbyColliders)
        {
            
            if (col.gameObject == gameObject) continue;
            
            Door door = col.GetComponent<Door>();
            if (door != null && !adjacentDoors.Contains(door))
            {
                adjacentDoors.Add(door);
            }
        }
    }
    
    /// Kiểm tra xem cửa có bị vật cản (Player, Box, v.v.) đè lên không
    private bool IsBlocked()
    {
        // Kiểm tra xem có vật cản trên cửa hiện tại không
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, detectionSize, 0f, blockingObjectsLayer);

        //Debug.Log("Door at " + transform.position + " checking blocked: found " + colliders.Length + " colliders.");

        if (colliders.Length > 0)
            return true;
        
        // Kiểm tra xem có vật cản trên các cửa liên kết không
        foreach(Door door in adjacentDoors)
        {
            if (door != null)
            {
                Collider2D[] adjacentColliders = Physics2D.OverlapBoxAll(door.transform.position, door.detectionSize, 0f, blockingObjectsLayer);
                if (adjacentColliders.Length > 0)
                    return true;
            }
        }
        
        return false;
    }
    
    
    // Vẽ vùng kiểm tra vật cản trong Scene view (để debug)
    private void OnDrawGizmosSelected()
    {
        // Đỏ: bị chặn, Xanh lá: mở, Vàng: đóng
        Gizmos.color = isBlocked ? Color.red : (isOpen ? Color.green : Color.yellow);
        Gizmos.DrawWireCube(transform.position, detectionSize);
        
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan trong suốt
        Gizmos.DrawWireSphere(transform.position, adjacentSearchRadius);
        
        // Vẽ đường nối đến các cửa liền kề
        Gizmos.color = Color.cyan;
        foreach (Door door in adjacentDoors)
        {
            if (door != null)
            {
                Gizmos.DrawLine(transform.position, door.transform.position);
            }
        }
    }

    public void ResetLevel()
    {
        isActived = false;
        isOpen = false;
        animator.SetBool("Open", false);
        doorCollider.enabled = true;
    }
}
