using System;
using Cysharp.Threading.Tasks;
using GamePlay.GridMap;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePlay.Enemy.Karen.Skills
{
    [Serializable]
    public class KarenSkill1 : BossSkill
    {
        [SerializeField] private float projectileSpeed;
        [SerializeField] private int projectileCountOneShot;
        [SerializeField] private int shootCount;
        [SerializeField] private float shootDelay;
        [Range(0f, 360f)]
        [SerializeField] private float shootAngle;

        [SerializeField] private Projectile projectilePrefab;


        protected override async UniTask ExecuteSkill()
        {
            for (var i = 0; i < shootCount; i++)
            {
                // 한번 발사
                await OneShot();
            }
        }

        private async UniTask OneShot()
        {
            // 발사 각도(중앙) 계산
            // 플레이어 위치(가운데 투사체 발사 벡터)
            var playerPos = GameManager.Instance.playerController.transform.position;
            var bossPos = GameManager.Instance.bossController.transform.position;
            var centerDir = (playerPos - bossPos).normalized;
            var halfAngle = shootAngle / 2;
            // 오브젝트 생성 및 발사
            for (var i = 0; i < projectileCountOneShot; i++)
            { 
                var t = (projectileCountOneShot <= 1) ? 0f : (float)i / (projectileCountOneShot - 1);
                var angle = Mathf.Lerp(-halfAngle, halfAngle, t);
                var dir = BossSkill.Rotate(centerDir, angle);

                var projectile = Object.Instantiate(projectilePrefab, bossPos, Quaternion.identity);
                projectile.SettingProjectile(projectileSpeed, dir, false);
            }
            // 다음 발사까지 기다리기
            await UniTask.Delay(TimeSpan.FromSeconds(shootDelay));
            
        }
    }
}
