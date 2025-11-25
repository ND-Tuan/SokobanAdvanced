using UnityEngine;
using System.Collections;

public class PlayerGridMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveDelay = 0.15f; // thời gian chờ giữa 2 bước khi giữ phím
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private float rayDistance = 0.6f;

    private bool isMoving = false;
    private Vector2 moveDir;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Snap vị trí ban đầu
        Vector3 p = transform.position;
        p.x = Mathf.RoundToInt(p.x) ;
        p.y = Mathf.RoundToInt(p.y);
        transform.position = p;
    }

    void Update()
    {
        if (!isMoving)
        {
            moveDir = Vector2.zero;

            // Ưu tiên theo chiều ngang
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(x) > Mathf.Abs(y))
                moveDir = new Vector2(Mathf.Sign(x), 0);
            else if (Mathf.Abs(y) > 0)
                moveDir = new Vector2(0, Mathf.Sign(y));

            if (moveDir != Vector2.zero)
            {
                Vector2 target = rb.position + moveDir;
                StartCoroutine(MoveToGrid(target, moveDir));
            }
        }
    }

    IEnumerator MoveToGrid(Vector2 target, Vector2 direction)
    {
        if (!Moveable(direction))
        {
            yield break; // Không di chuyển được
        }

        isMoving = true;

        // Di chuyển tới ô kế tiếp
        // safety guard: prevent infinite loop if movement is blocked mid-way
       
        float distance = Vector2.Distance(rb.position, target);
        float expectedTime = (moveSpeed > 0f) ? (distance / moveSpeed) : 0.5f;
        float maxTime = Mathf.Max(0.1f, expectedTime * 3f + 0.1f);
        float elapsed = 0f;

        while ((target - rb.position).sqrMagnitude > 0.000000001f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime));

            elapsed += Time.deltaTime;
            if (elapsed > maxTime)
            {
                Debug.LogWarning("MoveToGrid: movement timeout, snapping to nearest grid to avoid lockup.");

                // Snap to nearest integer grid cell to avoid being stuck mid-way
                target = new Vector2(Mathf.RoundToInt(rb.position.x), Mathf.RoundToInt(rb.position.y));
                gameObject.transform.position = target;
                isMoving = false;
                yield break;
            }

            yield return null;
        }

        transform.position = new Vector2(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));;

        // Chờ delay giữa các bước nếu người chơi vẫn giữ phím
        yield return new WaitForSeconds(moveDelay);

        // Nếu vẫn giữ cùng hướng → di tiếp ô kế tiếp
        if (IsHoldingDirection(direction))
        {
            Vector2 nextTarget = rb.position + direction;
            isMoving = false;

            StopAllCoroutines();
            StartCoroutine(MoveToGrid(nextTarget, direction));
        }
        else
        {
            isMoving = false;
        }

        isMoving = false;
    }

    private bool Moveable(Vector2 dir)
    {
        // Raycast kiểm tra vật phía trước
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayDistance, obstacleLayer);

        if (hit.collider == null) return true;
        
            // Nếu gặp hộp → thử đẩy
        // if (((1 << hit.collider.gameObject.layer) & boxLayer) != 0)
        // {
        //     // Kiểm tra ô sau hộp có trống không
        //     RaycastHit2D boxHit = Physics2D.Raycast(hit.collider.gameObject.transform.position, dir, rayDistance, obstacleLayer | boxLayer);
        //     if (boxHit.collider != null) return false; // bị chặn

        //     return true;

        // }
            // Nếu là tường hoặc bị chặn → không di chuyển
        else return false;
        
        
    }

    private bool IsHoldingDirection(Vector2 dir)
    {
        if (dir.x > 0) return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        if (dir.x < 0) return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        if (dir.y > 0) return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        if (dir.y < 0) return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        return false;
    }
}
