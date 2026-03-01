using System;
using DG.Tweening;
using UnityEngine;

public class OnTypingEndAction : MonoBehaviour
{
    [SerializeField] private Transform leftTransform;
    [SerializeField] private Transform rightTransform;
    private Tween leftTween;
    private Tween rightTween;

    private Vector3 leftPosition;
    private Vector3 rightPosition;
    
    private void Awake()
    {
        leftPosition = leftTransform.position;
        rightPosition = rightTransform.position;
    }

    private void OnEnable()
    {
        MoveTransforms();
    }

    private void OnDisable()
    {
        leftTween?.Kill();
        rightTween?.Kill();
    }

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveTime;
    
    private void MoveTransforms()
    {
        leftTransform.position = leftPosition;
        rightTransform.position = rightPosition;
        
        leftTween?.Kill();
        rightTween?.Kill();
        
        var startPosLeft = leftTransform.position;

        leftTween = leftTransform.DOMoveX(startPosLeft.x - moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        var startPosRight = rightTransform.position;
        rightTween = rightTransform.DOMoveX(startPosRight.x + moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
    }
}
