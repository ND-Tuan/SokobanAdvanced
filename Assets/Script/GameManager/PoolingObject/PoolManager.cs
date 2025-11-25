using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    private Dictionary<GameObject, Pool> poolDictionary = new Dictionary<GameObject, Pool>();
    private GameObject tempObject;


    private void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }
    }

    public GameObject Get(GameObject obj)
    {
        if (poolDictionary.ContainsKey(obj) == false)
        {
            poolDictionary.Add(obj, new Pool(obj, this.transform));
        }
        return poolDictionary[obj].Get();
    }

    public GameObject Get(GameObject obj, Vector3 position)
    {
        tempObject = Get(obj);
        tempObject.transform.position = position;
        return tempObject;
    }

}
