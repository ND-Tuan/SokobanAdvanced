using System.Collections;
using System.Collections.Generic;
using ObserverPattern;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPress : MonoBehaviour
{
    [SerializeField] private FxAudioDataSO buttonPressFx;


    private void Start()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);        

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(OnButtonPress);
        }
    }

    private void OnButtonPress()
    {
        Observer.PostEvent(EvenID.PlayFX, buttonPressFx);
    }

}
