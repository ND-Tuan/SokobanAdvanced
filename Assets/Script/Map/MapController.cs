using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private int moveLimit;

    [SerializeField] private int MoveToGetStar1;
    [SerializeField] private int MoveToGetStar2;
    [SerializeField] private int MoveToGetStar3;

    
    

    void Start()
    {
        SetUp();
    }

    private void SetUp(){
        GameManager.Instance.LevelManager.SetUpMap(this, moveLimit, new int[] { MoveToGetStar1, MoveToGetStar2, MoveToGetStar3 });

        
    }
    
    public void ResetMap()
    {
        SetUp();
        
        foreach(IResetLevel resetLevel in GetComponentsInChildren<IResetLevel>(true))
        {
            resetLevel.ResetLevel();
        }
    }
}




