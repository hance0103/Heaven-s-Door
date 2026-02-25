using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Enemy
{
    [Serializable]
    public abstract class BossSkill
    {
        [SerializeField] protected float beforeDelay;
        [SerializeField] protected float afterDelay;

        public virtual async UniTask UseSKill(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(beforeDelay));
            

            await ExecuteSkill();
            
            await UniTask.Delay(TimeSpan.FromSeconds(afterDelay));

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
