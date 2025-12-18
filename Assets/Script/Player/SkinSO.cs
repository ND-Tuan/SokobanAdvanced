using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinSO", menuName = "ScriptableObjects/SkinSO")]
public class SkinSO : ScriptableObject
{
    [SerializeField] private int skinID;
    [SerializeField] private string skinName;
    [SerializeField] private Sprite skinImage;
    [SerializeField] private int price;
    [SerializeField] private GameObject skinPrefab;
    [SerializeField] private bool specialEffect;

    public int SkinID => skinID;
    public string SkinName => skinName;
    public Sprite SkinImage => skinImage;
    public int Price => price;
    public GameObject SkinPrefab => skinPrefab;
    public bool SpecialEffect => specialEffect;
}

