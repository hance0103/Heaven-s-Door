using System;
using DG.Tweening;
using UnityEngine;

public class OnTypingEndAction : MonoBehaviour
{
    [SerializeField] private Transform leftTransform;
    [SerializeField] private Transform rightTransform;
    
    private void OnEnable()
    {
        MoveTransforms();
    }

    private void OnDisable()
    {
        
    }

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveTime;
    
    private void MoveTransforms()
    {
        var startPosLeft = leftTransform.position;

        leftTransform.DOMoveX(startPosLeft.x - moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        var startPosRight = rightTransform.position;
        rightTransform.DOMoveX(startPosRight.x + moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        
        
    }
}
