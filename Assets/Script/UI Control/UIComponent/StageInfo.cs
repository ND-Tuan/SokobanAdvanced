using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StageInfo : MonoBehaviour
{
    public int StageIndex => _stageIndex;
    public string StageName => _stageName;
    public Color MainColor => _mainColor;
    public Color BGColor => _bgColor;
    public CanvasGroup CanvasGroup => _canvasGroup;
    public AudioClip StageMainTheme => _stageMainTheme;

    [SerializeField] private int _stageIndex;
    [SerializeField] private string _stageName;
    [SerializeField] private Color _mainColor;
    [SerializeField] private Color _bgColor;
    [SerializeField] private AudioClip _stageMainTheme;
    private CanvasGroup _canvasGroup;


    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
}
