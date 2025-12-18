using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FxAudioData", menuName = "ScriptableObjects/FxAudioData")]
[Serializable]
public class FxAudioDataSO : ScriptableObject
{
    [SerializeField] private List<AudioClip> versionsList = new();
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    public List<AudioClip> VersionsList => versionsList;
    public float MinPitch => minPitch;
    public float MaxPitch => maxPitch;
}
