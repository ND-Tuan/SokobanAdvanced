using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstTutorial : MonoBehaviour
{
    [SerializeField] private GameObject firstTutorialPanel;
    [SerializeField] private Player player;
    private Vector3 playerStartPos;
    void Start()
    {
        StartCoroutine(DisplayGuide());
        playerStartPos = player.transform.position;
    }

    private IEnumerator DisplayGuide()
    {
        yield return new WaitForSeconds(0.5f);
        firstTutorialPanel.SetActive(true);

        yield return new WaitUntil(() => !player.isMoving && player.transform.position != playerStartPos);
        
        firstTutorialPanel.SetActive(false);
    }
}
