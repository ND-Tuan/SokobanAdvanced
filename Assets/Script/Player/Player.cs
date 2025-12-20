using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, IResetLevel, IMoveable
{
    [SerializeField] private GameObject Interface;
    [SerializeField] private ParticleSystem particle;
    private Animator animator;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private float rayDistance = 0.6f;


    private Vector2 startTouchPosition, endTouchPosition;
    public bool isMoving = false;
    
    private ParticleSystem[] Module;

    private Vector2 originalPosition;


    void Awake()
    {
        Interface.SetActive(false);
        Interface = null;

        GameObject currentSkin = PoolManager.Instance.Get(GameManager.Instance.PlayerDataManager.GetEquippedSkin().SkinPrefab);
        currentSkin.transform.SetParent(this.transform);

        Interface = currentSkin;
        Interface.transform.localPosition = Vector3.zero;
        Interface.transform.localScale = Vector3.one;

        particle = Interface.GetComponentInChildren<ParticleSystem>();
        originalPosition = transform.localPosition;
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        animator = Interface.GetComponent<Animator>();
        Module = particle.gameObject.GetComponentsInChildren<ParticleSystem>();

        // Snap vị trí ban đầu
        Vector3 p = transform.position;
        p.x = Mathf.RoundToInt(p.x) ;
        p.y = Mathf.RoundToInt(p.y);
        transform.position = p;
        
    }

    void Update()
    {
        if (!isMoving && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position; // Save start position
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position; // Save end position
                HandleSwipe(); // Handle swipe
            }
        }

         // Update animation state and effects
        if (animator != null) animator.SetBool("IsMoving", isMoving);
        SetActiveMainModule(isMoving);
    }

    void HandleSwipe()
    {
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;

        // Ignore if swipe is too short
        if (swipeDirection.magnitude < 30)
            return;

        // Determine swipe direction and move
        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
            StartCoroutine(MoveToGrid(swipeDirection.x > 0 ? Vector2.right : Vector2.left)); // Horizontal swipe
        else
            StartCoroutine(MoveToGrid(swipeDirection.y > 0 ? Vector2.up : Vector2.down)); // Vertical swipe
    }

    IEnumerator MoveToGrid(Vector2 direction)
    {
        if (!Moveable(direction))
        {
            yield break; // Không di chuyển được
        }

        isMoving = true;

        Vector2 target = rb.position + direction;

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

        transform.position = new Vector2(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));

        // GameManager.Instance.LevelManager.MinusMoveCount();

        isMoving = false;
    }

    private bool Moveable(Vector2 dir)
    {
        // Raycast kiểm tra vật phía trước
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayDistance, obstacleLayer);

        if (hit.collider == null) return true;
        
        else return false;   
    }

    private void SetActiveMainModule(bool active)
    {
        if (Module == null) return;
        
        foreach (ParticleSystem module in Module)
        {
            if (module == null) continue;
            
            var main = module.main;
            Color color = main.startColor.color;
            color.a = active ? 1f : 0f;
            main.startColor = color;
        }
    }

    public void ChangeDirection(Vector2 newDirection, Vector2 position)
    {
        StopAllCoroutines();
        transform.position = position;
        transform.position = position + newDirection;
        isMoving = false;
    }

    public void ResetLevel()
    {
        StopAllCoroutines();
        Invoke(nameof(ResetPosition), 0.1f);
        isMoving = false;

        Debug.Log("Player reset to original position: " + originalPosition);
    }

    private void ResetPosition()
    {
        transform.localPosition = originalPosition;
    }
}
