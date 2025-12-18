using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    public Pool pool;

    public void OnDisable()
    {
        if (pool != null && gameObject.activeInHierarchy == false)
        {
            pool.AddToPool(gameObject);
            gameObject.SetActive(false);
        }
    }
}
