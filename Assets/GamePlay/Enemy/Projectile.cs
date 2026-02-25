using System;
using GamePlay.Player;
using Managers;
using UnityEngine;

namespace GamePlay.Enemy
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private bool canPierce = false;
        
        private Rigidbody2D rb;
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void ShootProjectile(float speed, Vector2 moveVec, bool pierce)
        {
            // 해당하는 속도 및 방향으로 직진시키기
            rb.linearVelocity = moveVec * speed;
            canPierce = pierce;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.gameObject.name);
            
            if (other.gameObject.CompareTag("Collector")) Destroy(gameObject);
            
            if (!other.gameObject.CompareTag("Player")) return;
            
            // 관통이 가능하다면
            if (canPierce)
            {
                GameManager.Instance.inGameManager.MinusLife();
                return;
            }

            if (GameManager.Instance.playerController.Mode == TraverseMode.Border) return;
            
            GameManager.Instance.inGameManager.MinusLife();
        }
    }
}
