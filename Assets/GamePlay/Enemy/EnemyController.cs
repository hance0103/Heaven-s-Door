using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GamePlay.GridMap;
using GamePlay.Player;
using Managers;
using UnityEngine;

namespace GamePlay.Enemy
{
    public abstract class EnemyController : MonoBehaviour
    {
        [SerializeField] protected float moveSpeed;

        [SerializeField] protected float moveDuration;
        
        [SerializeField] protected Vector2 moveDirection;
        
        protected Rigidbody2D Rigidbody;
        protected SpriteRenderer Sprite;
        public enum BossState
        {
            Idle,
            Move,
            Attack,
            GameEnd
        }
    
        // 보스 중앙 노드 (위치)
        [SerializeField] protected Vector2Int currentNode;
        public Vector2Int CurrentNode => currentNode;
    
        [SerializeField] protected BossState bossState;

        private void Awake()
        {  
            Sprite =  GetComponent<SpriteRenderer>();
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (bossState == BossState.Move)
            {
                RenewBossNode();
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void Start()
        {
            transform.position = GridMath.NodeToWorld(
                currentNode.x, 
                currentNode.y, 
                GameManager.Instance.gridManager.Origin, 
                GameManager.Instance.gridManager.CellWorldSize);
            GameManager.Instance.bossController = this;
        }

        public abstract void SetNextState(BossState state);

        public void SetBossState(BossState state)
        {
            bossState = state;
        }
        protected async UniTask WaitForSeconds(float seconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
        }

        private void RenewBossNode()
        {
            var girdManager = GameManager.Instance.gridManager;
            if (girdManager == null) return;
            
            var bossPos = transform.position;
            currentNode = girdManager.GetWorldToNode(new Vector2(bossPos.x, bossPos.y));
        }
        protected virtual void MoveToPlayer()
        {
            bossState = BossState.Move;
            
            var player = GameManager.Instance.playerController;
            
            if (player == null) return;
            
            var playerPos = player.transform.position;
            var bossPos = transform.position;
            
            moveDirection = (playerPos - bossPos).normalized;
            Rigidbody.linearVelocity = moveDirection * moveSpeed;
            
        }
        
        protected virtual void ChangeMoveDir()
        {
            
        }

        [Header("보스 사망")]
        [SerializeField] private GameObject bossDeathEffectPrefab;
        [SerializeField] private float effectScale;
        [SerializeField] private List<Vector2> bossEffectPostion;
        
        [SerializeField] private float timeBetweenEffect;
        private List<GameObject> effectObejcts = new();
        public async UniTask OnBossDead()
        {


            for (var i = 0; i < bossEffectPostion.Count; i++)
            {
                var localPos = transform.position + new Vector3(bossEffectPostion[i].x, bossEffectPostion[i].y, 0);
                
                var effectObject = Instantiate(bossDeathEffectPrefab, localPos, Quaternion.identity, transform);
                effectObject.transform.localScale = Vector3.one * effectScale;
                effectObejcts.Add(effectObject);

                if (i == bossEffectPostion.Count - 1)
                {
                    Sprite.color = new Color(1, 1, 1, 0);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(timeBetweenEffect));
                }

            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            foreach (var obj in effectObejcts)
            {
                Destroy(obj);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            
            var player = GameManager.Instance.playerController;
            
            if (player == null || player.IsInvincible) return;
            
            if (player.Mode != TraverseMode.Border)
                GameManager.Instance.inGameManager.MinusLife();
        
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("CapturedBoundary") || bossState != BossState.Move) return;
            
            var contact = other.GetContact(0);
            
            var inDir = moveDirection;   // 입사 방향
            var normal = contact.normal;           // 충돌면 법선

            var reflectDir = Vector2.Reflect(inDir, normal);

            moveDirection = reflectDir;
            Rigidbody.linearVelocity = moveDirection * moveSpeed;

        }
    }
}
