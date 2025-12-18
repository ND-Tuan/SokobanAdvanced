using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : BasePanelController
{
    static Dictionary<EvenID, List<Action<object[]>>> Listener = new Dictionary<EvenID, List<Action<object[]>>>();


    public static void AddListener(EvenID evenID, Action<object[]> action)
    {
        Listener[evenID].Sort();

    }

    
}

