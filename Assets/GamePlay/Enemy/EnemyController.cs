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
        public enum BossState
        {
            Idle,
            Move,
            Attack,
            Wait
        }
    
        // 보스 중앙 노드 (위치)
        [SerializeField] protected Vector2Int currentNode;
        public Vector2Int CurrentNode => currentNode;
    
        [SerializeField] protected BossState bossState;
        
        
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


        protected async UniTask WaitForSeconds(float seconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
        }
    
    
        private async UniTask MoveToNode(Vector2Int node)
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
    }
}
