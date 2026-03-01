using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;

public class GameStartEffect : MonoBehaviour
{
    [SerializeField] private GameObject _overlayCanvas;
    [SerializeField] private GameObject _ingameObject;
    [SerializeField] private List<Transform> effectWords;
    private void Awake()
    {
        SetGameObjects(false);
    }

    private async void Start()
    {
        foreach (var word in effectWords)
        {
            word.gameObject.SetActive(false);
        }
        
        await StartEffect();
        SetGameObjects(true);
    }

    private async UniTask StartEffect()
    {
        foreach (var word in effectWords)
        {
            await EffectOfOneWord(word);
        }
        gameObject.SetActive(false);
    }

    [SerializeField] private float effectTime = 1.5f;
    [SerializeField] private float popScale = 1.3f;

    private Tween _scaleTween;
    private async UniTask EffectOfOneWord(Transform wordTransform)
    {
        wordTransform.gameObject.SetActive(true);
        wordTransform.localScale *= popScale;
        _scaleTween?.Kill();

        _scaleTween = wordTransform
            .DOScale(Vector3.one, effectTime)
            .SetEase(Ease.Linear)
            .SetLink(gameObject);
        
        await UniTask.Delay(TimeSpan.FromSeconds(effectTime));
        _scaleTween.Kill();
        wordTransform.localScale = Vector3.one;
    }

    private void SetGameObjects(bool value)
    {
        _overlayCanvas.SetActive(value);
        _ingameObject.SetActive(value);
        

    }
}
