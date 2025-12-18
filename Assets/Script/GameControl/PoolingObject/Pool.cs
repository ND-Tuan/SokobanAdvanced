using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Pool
{
    private Stack<GameObject> objectStack = new Stack<GameObject>();  
    private GameObject prefab;                                        
    private ReturnToPool returnToPool;                       
    private GameObject instance;
    private GameObject poolGameObject; 
                                       

    public Pool(GameObject prefab, Transform poolParent)
    {
        this.prefab = prefab;

        poolGameObject = new GameObject(prefab.name + "_Pool");
        poolGameObject.transform.parent = poolParent;

    }

    public GameObject Get()
    {
        while (objectStack.Count > 0)
        {
            instance = objectStack.Pop();
            if (instance != null)
            {
                instance.SetActive(true);
                instance.transform.parent = null; // Remove from pool

                return instance;
            }
            else
            {
                Debug.LogWarning($"[Pool] GameObject '{prefab.name}' has been destroyed!");
            }
        }

        // Create new instance if stack is empty
        instance = GameObject.Instantiate(prefab);
        returnToPool = instance.AddComponent<ReturnToPool>();
        returnToPool.pool = this;
        return instance;
    }

    
    /// Return object back to the pool.
    public async void AddToPool(GameObject obj)
    {
        await Task.Delay(1); // Wait 1 frame to ensure the object is fully deactivated
        if (obj == null) return;
        obj.transform.parent = poolGameObject.transform; // Set as child of poolGameObject
        objectStack.Push(obj);
    }
}