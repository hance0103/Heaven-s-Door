using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePlay.Enemy.Karen.Skills
{
    [Serializable]
    public class KarenSkill2 : BossSkill
    {
        [SerializeField] private float projectileSpeed;
        [SerializeField] private int shootCount;
        [SerializeField] private float creatDelay;
        [SerializeField] private float shootDelay;
        [SerializeField] private float createDistanceFromBoss;
        [SerializeField] private float createDistanceBetweenProjectiles;
    
        [SerializeField] private Projectile projectilePrefab;
    
        protected override async UniTask ExecuteSkill()
        {
            await CreateAndShootProjectiles();
            await UniTask.Delay(TimeSpan.FromSeconds(creatDelay));
            
        }

        private async UniTask CreateAndShootProjectiles()
        {
            var playerPos = GameManager.Instance.playerController.transform.position;
            var bossPos = GameManager.Instance.bossController.transform.position;
            
            var toPlayerDir =  (bossPos - playerPos).normalized;

            var centerProjectilePos = bossPos + toPlayerDir * createDistanceFromBoss;

            // var projectileCreateDir = new Vector3(toPlayerDir.y, -toPlayerDir.x);
            
            List<Projectile> projectiles = new List<Projectile>();
            for (var i = 0; i < shootCount; i++)
            {
                var projectile = Object.Instantiate(projectilePrefab, centerProjectilePos, Quaternion.identity);
                projectiles.Add(projectile);
                
                
                //await UniTask.Delay(TimeSpan.FromSeconds(creatDelay));
            }


            foreach (var projectile in projectiles)
            {
                var projectileVec = GameManager.Instance.playerController.transform.position - projectile.transform.position;
                projectile.ShootProjectile(projectileSpeed, projectileVec, false);
                await UniTask.Delay(TimeSpan.FromSeconds(shootDelay));
            }

        }
    }
}
