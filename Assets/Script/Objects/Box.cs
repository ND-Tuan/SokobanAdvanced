using UnityEngine;

public class Box : MonoBehaviour
{
	[SerializeField] private float gridSize = 1f;
	[SerializeField] private float gridOffset = 0f;
	[SerializeField] private Rigidbody2D rb;
	[SerializeField] private float velocitySnapThreshold = 0.01f;

	private bool isSnapped = false;

	void Start()
	{
		if (rb == null) rb = GetComponent<Rigidbody2D>();
		isSnapped = true;
	}

	void FixedUpdate()
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Box OnCollisionEnter2D with " + other.gameObject.name);
        if(other.gameObject.CompareTag("Box"))
        {
            rb.mass = 10000f;
        } else
        {
            rb.mass = 0.1f;
        }

    }
}
