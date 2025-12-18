using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private int moveLimit;
    [SerializeField] private int MoveToGetStar1;
    [SerializeField] private int MoveToGetStar2;
    [SerializeField] private int MoveToGetStar3;

    [SerializeField] private List<GuideDisplayInfo> guideNeedToDisplayList = new();

    public List<FinishTile> finishTiles = new List<FinishTile>();

    void Awake()
    {

        Observer.AddListener(EvenID.CompleteBox, WinCheck);
    }

    void Start()
    {
        SetUp();
        finishTiles = new List<FinishTile>(GetComponentsInChildren<FinishTile>());
    }

    private void SetUp(){
        GameManager.Instance.LevelManager.SetUpMap(this, moveLimit, new int[] { MoveToGetStar1, MoveToGetStar2, MoveToGetStar3 });

        if(guideNeedToDisplayList.Count > 0)
        {
            StartCoroutine(DisplayGuide());
        }
    }
    
    private IEnumerator DisplayGuide()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.5f);
        UIController.Instance.DisplayGuide(guideNeedToDisplayList);
    }

    public void ResetMap()
    {
        SetUp();
        
        foreach(IResetLevel resetLevel in GetComponentsInChildren<IResetLevel>(true))
        {
            resetLevel.ResetLevel();
        }
    }

   

    private void WinCheck(object[] data)
    {
        foreach(FinishTile finishTile in finishTiles)
        {
            if(!finishTile.isCompleted)
            {
                return;
            }
        }

        GameManager.Instance.GameWin(); 
    }
}
public static class ColorChanger
{
    public static void ChangeColor(this SpriteRenderer spriteRenderer, int colorID)
    {
        colorID = Mathf.Clamp(colorID, 0, 10);
        spriteRenderer.color = distinctColors[colorID];
    }

    public static Color GetColorByID(int colorID)
    {
        colorID = Mathf.Clamp(colorID, 0, 10);
        return distinctColors[colorID];
    }


    private static readonly Color[] distinctColors = new Color[]
    {
        new Color(1f, 0, 0.2361317f),       
        new Color(1, 0.827451f, 0, 1),   
        new Color(0, 1, 0.4769833f, 1),         
        new Color(0, 0.827451f, 1, 1),           
        new Color(1f, 0f, 1f), // Magenta - 4
        new Color(1f, 0.5f, 0.5f), // Pink - 5
        new Color(1f, 0, 0.5f),// Rose - 6
        new Color(1f, 0.5f, 0f), // Orange - 7
        new Color(0.5f, 0f, 1f), // Purple - 8
        new Color(0f, 0.5f, 1f), // Sky Blue - 9
        Color.green
    };
}


