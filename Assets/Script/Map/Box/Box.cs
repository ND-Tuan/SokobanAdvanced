using System.Collections;
using ObserverPattern;
using UnityEngine;

public class Box : MonoBehaviour, IMoveable, IResetLevel
{
	[SerializeField] protected float gridSize = 1f;
	[SerializeField] protected float gridOffset = 0f;
	[SerializeField] protected Rigidbody2D rb;
	[SerializeField] protected float velocitySnapThreshold = 0.01f;
	[SerializeField] protected FxAudioDataSO MoveFailAudioData;
	[SerializeField] protected FxAudioDataSO MoveAudioData;
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
		//tự động snap box khi vận tốc thấp
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
    
	//Lấy vị trí đã khớp gần nhất
    private Vector2 GetSnappedPosition(Vector2 pos)
	{
		if (gridSize == 0f) return pos;
		float x = Mathf.Round((pos.x - gridOffset) / gridSize) * gridSize + gridOffset;
		float y = Mathf.Round((pos.y - gridOffset) / gridSize) * gridSize + gridOffset;
		return new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
	}

	//Xử lý va chạm với vật thể khác
    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Box OnCollisionEnter2D with " + other.gameObject.name);

		//Điều chỉnh khối lượng để tránh người chơi đẩy 2 box cùng lúc
        if(other.gameObject.CompareTag("Player"))
        {
            rb.mass = 0.1f;
			StartCoroutine(CheckToMinusMoveCount(transform.position));
            Observer.PostEvent(EvenID.PlayFX, MoveAudioData);

        } else
        {
            rb.mass = 10000;
        }

		if (other.gameObject.CompareTag("MagneticBox") 
			|| other.gameObject.CompareTag("FireBox") 
			|| other.gameObject.CompareTag("ElectricBox") 
			|| other.gameObject.CompareTag("StandardBox"))
		{
			Observer.PostEvent(EvenID.PlayFX, MoveFailAudioData);
		}

    }


	protected IEnumerator CheckToMinusMoveCount(Vector2 oldPosition)
	{
		yield return new WaitForSeconds(0.2f);

		if (Vector2.Distance(oldPosition, transform.position) >= 0.3f)
		{
			GameManager.Instance.LevelManager.MinusMoveCount();
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
