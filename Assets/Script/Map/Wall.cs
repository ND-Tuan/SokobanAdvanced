using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
   void OnValidate()
   {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (collider != null)
        {
            collider.size = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y-0.25f);
        }
   }
}
