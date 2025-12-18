using System.Collections.Generic;
using UnityEngine;

public class MagnetBox : Box
{
    [SerializeField] private float magnetDistance = 1.1f; // Khoảng cách để dính với nhau
    [SerializeField] private LayerMask magnetBoxLayer; // Layer để phát hiện MagnetBox khác
    
    public List<MagnetBox> connectedBoxes = new List<MagnetBox>();
    private List<FixedJoint2D> joints = new List<FixedJoint2D>();
    private bool isUpdatingConnections = false;


    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!isUpdatingConnections)
        {
            UpdateMagnetConnections();
        }
    }

    private void UpdateMagnetConnections()
    {
        isUpdatingConnections = true;

        // Tìm tất cả MagnetBox gần đây
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, magnetDistance);
        List<MagnetBox> nearbyMagnetBoxes = new List<MagnetBox>();

        foreach (Collider2D col in nearbyColliders)
        {
            if (col.gameObject != gameObject)
            {
                MagnetBox otherMagnet = col.GetComponent<MagnetBox>();
                if (col.CompareTag("MagneticBox"))
                {
                    float distance = Vector2.Distance(transform.position, otherMagnet.transform.position);
                    if (distance <= magnetDistance && IsAdjacent(otherMagnet))
                    {
                        nearbyMagnetBoxes.Add(otherMagnet);
                    }
                }
            }
        }

        // Thêm kết nối mới
        foreach (MagnetBox magnetBox in nearbyMagnetBoxes)
        {
            if (!connectedBoxes.Contains(magnetBox))
            {
                ConnectBox(magnetBox);
            }
        }

        isUpdatingConnections = false;
    }

    private bool IsAdjacent(MagnetBox other)
    {
        Vector2 diff = other.transform.position - transform.position;
        float absDiffX = Mathf.Abs(diff.x);
        float absDiffY = Mathf.Abs(diff.y);

        bool horizontallyAdjacent = (absDiffX >= 0.9f && absDiffX <= 1.1f) && absDiffY < 0.2f;
        bool verticallyAdjacent = (absDiffY >= 0.9f && absDiffY <= 1.1f) && absDiffX < 0.2f;

        return horizontallyAdjacent || verticallyAdjacent;
    }

    private void ConnectBox(MagnetBox other)
    {
        if (other == null || connectedBoxes.Contains(other)) return;

        connectedBoxes.Add(other);

        // Tạo FixedJoint2D để dính 2 box lại với nhau
        FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = other.GetComponent<Rigidbody2D>();
        joint.enableCollision = false;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        
        joints.Add(joint);
    }

    protected override void DoAnythingElse()
    {
        DisconnectAllBoxes();
    }

    private void DisconnectAllBoxes()
    {
        // Ngắt kết nối tất cả các box
        for (int i = connectedBoxes.Count - 1; i >= 0; i--)
        {
            DisconnectBox(connectedBoxes[i]);
        }
    }

    private void DisconnectBox(MagnetBox other)
    {
        if (other == null) return;

        int index = connectedBoxes.IndexOf(other);
        if (index != -1)
        {
            // Xóa joint trước
            if (index < joints.Count && joints[index] != null)
            {
                Destroy(joints[index]);
                joints[index] = null;
            }
            
            // Xóa khỏi danh sách
            connectedBoxes.RemoveAt(index);
            if (index < joints.Count)
            {
                joints.RemoveAt(index);
            }
            
            // Đảm bảo box kia cũng ngắt kết nối với mình
            if (other.connectedBoxes.Contains(this))
            {
                int otherIndex = other.connectedBoxes.IndexOf(this);
                if (otherIndex != -1)
                {
                    if (otherIndex < other.joints.Count && other.joints[otherIndex] != null)
                    {
                        Destroy(other.joints[otherIndex]);
                        other.joints[otherIndex] = null;
                    }
                    other.connectedBoxes.RemoveAt(otherIndex);
                    if (otherIndex < other.joints.Count)
                    {
                        other.joints.RemoveAt(otherIndex);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Dọn dẹp tất cả các joints khi object bị phá hủy
        foreach (FixedJoint2D joint in joints)
        {
            if (joint != null)
            {
                Destroy(joint);
            }
        }
        joints.Clear();
        connectedBoxes.Clear();
    }

    // Override để xử lý va chạm - MagnetBox chỉ kết nối với MagnetBox
    protected override void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("MagnetBox OnCollisionEnter2D with " + other.gameObject.name);
        
        if (other.gameObject.CompareTag("MagneticBox"))
            return;

        if(other.gameObject.CompareTag("Player"))
        {
            SyncMassToGroup(0.1f);
        } else
        {
            SyncMassToGroup(10000f);
        }
    }

    // Đồng bộ mass cho tất cả các box trong nhóm
    private void SyncMassToGroup(float newMass)
    {
        // Đặt mass cho box hiện tại
        rb.mass = newMass;

        // Đồng bộ mass cho tất cả các box kết nối
        HashSet<MagnetBox> visited = new HashSet<MagnetBox>();
        Queue<MagnetBox> toProcess = new Queue<MagnetBox>();
        
        visited.Add(this);
        toProcess.Enqueue(this);

        while (toProcess.Count > 0)
        {
            MagnetBox current = toProcess.Dequeue();
            
            foreach (MagnetBox connected in current.connectedBoxes)
            {
                if (connected != null && !visited.Contains(connected))
                {
                    visited.Add(connected);
                    connected.rb.mass = newMass;
                    toProcess.Enqueue(connected);
                }
            }
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, magnetDistance);

        foreach (MagnetBox connected in connectedBoxes)
        {
            if (connected != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, connected.transform.position);
            }
        }
    }
}
