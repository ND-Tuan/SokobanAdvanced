using System.Collections.Generic;
using ObserverPattern;
using UnityEngine;

public class FinishTile : MonoBehaviour
{
    enum FinishTileType
    {
        Standard,
        Electric,
        Fire,
        Magnetic
    }
    [SerializeField] private FinishTileType finishTileType;
    [SerializeField] private List<Sprite> finishTileSprites;
    [SerializeField] private List<Sprite> iconSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer IconSpriteRenderer;
    public bool isCompleted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        string expectedTag = finishTileType switch
        {
            FinishTileType.Fire => "FireBox",
            FinishTileType.Electric => "ElectricBox",
            FinishTileType.Magnetic => "MagneticBox",
            _ => ""
        };

        if (other.CompareTag(expectedTag) 
                || (finishTileType == FinishTileType.Standard 
                    && (other.CompareTag("StandardBox") 
                        || other.CompareTag("MagneticBox") 
                        || other.CompareTag("ElectricBox") 
                        || other.CompareTag("FireBox")
                        )
                    )
                )
        {
            isCompleted = true;
            Observer.PostEvent(EvenID.CompleteBox, null);
            IconSpriteRenderer.sprite = iconSprites[4];
            IconSpriteRenderer.gameObject.SetActive(true);
            return;
        }

        if (other.CompareTag("Player")) return;

        IconSpriteRenderer.sprite = iconSprites[(int)finishTileType];
        IconSpriteRenderer.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        IconSpriteRenderer.gameObject.SetActive(false);
        isCompleted = false;
    }
    
    #if UNITY_EDITOR
    void OnValidate()
    {
        spriteRenderer.sprite = finishTileSprites[(int)finishTileType]; 
    }
    #endif

}
