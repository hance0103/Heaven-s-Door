using System;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePlay.Enemy.Karen.Skills
{
    [Serializable]
    public class KarenSkill3 : BossSkill
    {
        [SerializeField] private float speed;
        [SerializeField] private float duration;
        [SerializeField] private float distanceFromBoss;
        [SerializeField] private float delayBetweenSpawn;
        
        [SerializeField] private SpawnObject spawnObjectPrefab;
        protected override async UniTask ExecuteSkill()
        { 
            CreateAndReleaseSpawnObject();
            await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenSpawn));
            CreateAndReleaseSpawnObject();
        }

        private void CreateAndReleaseSpawnObject()
        {
            var createPos = SelectCreatePosition().normalized * distanceFromBoss + 
                            GameManager.Instance.bossController.transform.position;
            
            var playerPos = GameManager.Instance.playerController.transform.position;
            var spawnObject = Object.Instantiate(spawnObjectPrefab, createPos, Quaternion.identity);
            spawnObject.SpawnObjectSetting(playerPos - createPos, speed, duration);
            _ = spawnObject.ReleaseSpawnObject();
        }

        private Vector3 SelectCreatePosition()
        {
            Vector2[] dirs =
            {
                new Vector2(1, 0),   // 0 → 오른쪽
                new Vector2(1, 1),   // 1 → 오른쪽 위
                new Vector2(0, 1),   // 2 → 위
                new Vector2(-1, 1),  // 3 → 왼쪽 위
                new Vector2(-1, 0),  // 4 → 왼쪽
                new Vector2(-1, -1), // 5 → 왼쪽 아래
                new Vector2(0, -1),  // 6 → 아래
                new Vector2(1, -1),  // 7 → 오른쪽 아래
            };
            
            var rand = UnityEngine.Random.Range(0, 8);
            return dirs[rand];
        }
    }
}
