using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTutorial : MonoBehaviour
{
    [SerializeField] private List<GuideDisplayInfo> guideNeedToDisplayList = new();
    [SerializeField] private GameObject firstTutorialPanel;
    [SerializeField] private Player player;
    void Start()
    {
        StartCoroutine(DisplayGuide());
    }

    private IEnumerator DisplayGuide()
    {
        yield return new WaitForSeconds(0.5f);
        firstTutorialPanel.SetActive(true);

        yield return new WaitUntil(() => !player.isMoving && player.transform.position != new Vector3(-3, -3, 0));
        
        firstTutorialPanel.SetActive(false);
    }
}
