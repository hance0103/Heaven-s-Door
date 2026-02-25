using System;
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
        protected Rigidbody2D rb;
        public enum BossState
        {
            Idle,
            Move,
            Attack,
            Wait,
            GameEnd
        }
    
        // 보스 중앙 노드 (위치)
        [SerializeField] protected Vector2Int currentNode;
        public Vector2Int CurrentNode => currentNode;
    
        [SerializeField] protected BossState bossState;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
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
            var bossPos = transform.position;
            currentNode = GameManager.Instance.gridManager.GetWorldToNode(new Vector2(bossPos.x, bossPos.y));
        }
        protected virtual async UniTask MoveToPlayer()
        {
            bossState = BossState.Move;
            
            var playerPos = GameManager.Instance.playerController.transform.position;
            var bossPos = transform.position;
            
            var moveDir = (playerPos - bossPos).normalized;
            rb.linearVelocity = moveDir * moveSpeed;
            await UniTask.Delay(TimeSpan.FromSeconds(moveDuration));
            
            rb.linearVelocity = Vector2.zero;
            
        }
        
        protected virtual void ChangeMoveDir()
        {
            
        }
        public void OnBossDead()
        {
            
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
        
            if (GameManager.Instance.playerController.Mode != TraverseMode.Border)
                GameManager.Instance.inGameManager.MinusLife();
        
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("CapturedBoundary")) return;

        }
    }
}
