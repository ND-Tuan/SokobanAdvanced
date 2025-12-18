using System.Collections;
using ObserverPattern;
using UnityEngine;

public class ConveyorTile : MonoBehaviour, IResetLevel, IPowerRequire
{
    [SerializeField] private bool isPowered = true;
    [Range(0, 3)]
    [SerializeField] private int NavigatorID;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Diraction diraction;
    private Diraction originalDiraction;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private GameObject EnergyRing;


    private void Awake()
    {
        Observer.AddListener(EvenID.ChangeDiraction, SetDiraction);
        originalDiraction = diraction;
        spriteRenderer.gameObject.SetActive(isPowered);
    }


    //Đổi hướng người chơi
    void OnTriggerEnter2D(Collider2D other)
    {
        Observer.PostEvent(EvenID.ReportTaskProgress, new object[] { TaskType.UseNavigator, 1, true });   
        StartCoroutine(WaitAndChangeDirection(other));
        
    }

    IEnumerator WaitAndChangeDirection(Collider2D other)
    {
        yield return new WaitUntil(() => Vector2.Distance(other.transform.position, transform.position) < 0.01f);

         Vector2 otherDirection = new();

        switch (diraction)
        {
            case Diraction.Up:
                otherDirection = Vector2.up;
                break;
            case Diraction.Down:
                otherDirection = Vector2.down;
                break;
            case Diraction.Left:
                otherDirection = Vector2.left;
                break;
            case Diraction.Right:
                otherDirection = Vector2.right;
                break;
        }

        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + otherDirection*0.55f, otherDirection, 0.9f, obstacleLayer);

        if (hit.collider != null ) yield break;

        IMoveable Object = other.GetComponent<IMoveable>();
        Object.ChangeDirection(otherDirection, transform.position);
    }


    //Đổi hướng của Navigator
    private void ChangeDiraction()
    {
        switch (diraction)
        {
            case Diraction.Up:
                gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case Diraction.Down:
                gameObject.transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case Diraction.Left:
                gameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
            case Diraction.Right:
                gameObject.transform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }

    private void SetDiraction(object[] data)
    {
        if ((int)data[0] == NavigatorID)
        {
            if (data[1] == null)
                diraction = originalDiraction;
            else
                diraction = (Diraction)data[1];

            ChangeDiraction();
        }
    }

    public void SetPowerState(bool state)
    {
        isPowered = state;
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);
        spriteRenderer.gameObject.SetActive(isPowered);

    }

    public void ResetLevel()
    {
        diraction = originalDiraction;
        ChangeDiraction();
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        ChangeDiraction();
        ColorChanger.ChangeColor(spriteRenderer, NavigatorID);
        boxCollider2D.enabled = isPowered;
        EnergyRing.SetActive(isPowered);
    }
    #endif

    private void OnDestroy()
    {
        Observer.RemoveListener(EvenID.ChangeDiraction, SetDiraction);
    }

    private void OnDrawGizmos()
    {

        Vector2 otherDirection = new();

         switch (diraction)
        {
            case Diraction.Up:
                otherDirection = Vector2.up;
                break;
            case Diraction.Down:
                otherDirection = Vector2.down;
                break;
            case Diraction.Left:
                otherDirection = Vector2.left;
                break;
            case Diraction.Right:
                otherDirection = Vector2.right;
                break;
        }
        Gizmos.color = Color.yellow;
        Vector2 startPos = (Vector2)transform.position + otherDirection * 0.6f;
        Vector2 endPos = startPos + otherDirection * 0.9f;
        Gizmos.DrawLine(startPos, endPos);
    }
}

public enum Diraction
{
    Up,
    Down,
    Left,
    Right
}
