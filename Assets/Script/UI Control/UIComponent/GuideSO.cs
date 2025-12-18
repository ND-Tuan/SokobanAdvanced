using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideSO", menuName = "ScriptableObjects/Guide")]
public class GuideSO : ScriptableObject
{   
    [SerializeField] private GuideType guideType;
    [SerializeField] private string guideName;
    [SerializeField] private string guideDescription;


    public GuideType GuideType => guideType;
    public string GuideName => guideName;
    public string GuideDescription => guideDescription;

}

public enum GuideType
{
    GoalTile,
    Door,
    DoorPressure,
    PressurePlate,
    MagnetBox,
    FireBox,
    ElectricBox,
    OilBarrel,
    Teleporter,
    ConveyorBelt,
    PowerDelivery

}
    
