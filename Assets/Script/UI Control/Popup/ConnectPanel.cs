using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPanel : BasePanelController
{

    [Header("Main Components")]
    [SerializeField] private GameObject ConnectButton;
    [SerializeField] private GameObject Disconnect;
    [SerializeField] private GameObject Connecting;
    [SerializeField] private GameObject LoadingIcon;



    void Update()
    {
        if (Connecting.activeInHierarchy)
        {
            LoadingIcon.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 200);
        }
    }

    public override void Hide()
    {
        PopOut(AnimationTimeOut);
    }

    public override void Show()
    {
        Popup(AnimationTimeIn);
    }

    public void ShowConnecting()
    {
        ConnectButton.SetActive(false);
        Disconnect.SetActive(false);
        Connecting.SetActive(true);
    }

    public void ShowDisconnect()
    {
        ConnectButton.SetActive(true);
        Disconnect.SetActive(true);
        Connecting.SetActive(false);
    }

    public void OnRetryConnect()
    {
        TimeManager.OnRetryClicked();
        ShowConnecting();
    }
}
