using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Managers;
using UnityEngine;

namespace GamePlay.Enemy
{
    [Serializable]
    public abstract class BossSkill
    {
        [SerializeField] protected float beforeDelay;
        [SerializeField] protected Color delayColor1;
        [SerializeField] protected Color delayColor2;
        [SerializeField] protected float timeBetweenColors;
        [SerializeField] protected float afterDelay;


        private Tween _colorTween;
        public virtual async UniTask UseSKill(CancellationToken token)
        {
            var bossRender = GameManager.Instance.bossController.sprite;
            
            var seq = DOTween.Sequence();

            seq.Append(bossRender.DOColor(delayColor1, timeBetweenColors));
            seq.Append(bossRender.DOColor(delayColor2, timeBetweenColors));
            seq.SetLoops(-1, LoopType.Yoyo);
            
            await UniTask.Delay(TimeSpan.FromSeconds(beforeDelay), cancellationToken: token);
            seq.Kill();
            
            bossRender.color = Color.white;

            await ExecuteSkill();
            
            await UniTask.Delay(TimeSpan.FromSeconds(afterDelay), cancellationToken: token);

        }

        protected abstract UniTask ExecuteSkill();
        
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
    }
}
