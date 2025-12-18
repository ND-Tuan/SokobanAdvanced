using System.Collections;
using UnityEngine;

public class Box : MonoBehaviour, IMoveable, IResetLevel
{
	[SerializeField] protected float gridSize = 1f;
	[SerializeField] protected float gridOffset = 0f;
	[SerializeField] protected Rigidbody2D rb;
	[SerializeField] protected float velocitySnapThreshold = 0.01f;
	private Vector2 originalPosition;

	private bool isSnapped = false;

	void Start()
	{
		isSnapped = true;
		originalPosition = transform.position;
	}

	protected virtual void FixedUpdate()
	{
		SnapHandler();
	}

	protected virtual void SnapHandler()
	{
		// Auto-snap logic when box comes to rest
		// using Rigidbody2D velocity to detect stop
		if (rb.linearVelocity.sqrMagnitude > velocitySnapThreshold * velocitySnapThreshold)
		{
			isSnapped = false;
		}
		else
		{
			if (!isSnapped){
				// snap immediately when nearly stopped
				transform.position = GetSnappedPosition(transform.position);
				isSnapped = true;
			}
		}
	}
    

    private Vector2 GetSnappedPosition(Vector2 pos)
	{
		if (gridSize == 0f) return pos;
		float x = Mathf.Round((pos.x - gridOffset) / gridSize) * gridSize + gridOffset;
		float y = Mathf.Round((pos.y - gridOffset) / gridSize) * gridSize + gridOffset;
		return new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
	}

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Box OnCollisionEnter2D with " + other.gameObject.name);
        if(other.gameObject.CompareTag("Player"))
        {
            rb.mass = 0.1f;
        } else
        {
            rb.mass = 10000;
        }

    }

	public void ChangeDirection(Vector2 newDirection, Vector2 position)
    {
        StopAllCoroutines();
		DoAnythingElse();
        transform.position = position;
        transform.position = position + newDirection;
		
        
    }

	protected virtual void DoAnythingElse()
	{
		// For override

	}

	public void ResetLevel()
	{
		transform.position = originalPosition;
		DoAnythingElse();
	}
}
