using System;
using Cysharp.Threading.Tasks;
using GamePlay.Player;
using Managers;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    
    private Rigidbody2D rb;
    private Vector3 moveDirection;
    private float moveSpeed;
    private float duration;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SpawnObjectSetting(Vector3 direction, float speed, float time)
    {
        moveDirection = direction;
        moveSpeed = speed;
        duration = time;
    }

    public async UniTask ReleaseSpawnObject()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
        
        // 지속시간이 다 되면 제거
        await UniTask.Delay(
            TimeSpan.FromSeconds(duration),
            cancellationToken: this.GetCancellationTokenOnDestroy()
        );
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        var player = GameManager.Instance.playerController;
        if (player == null || player.IsInvincible) return;
        
        if (player.Mode != TraverseMode.Border)
            GameManager.Instance.inGameManager.MinusLife();
        
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("CapturedBoundary")) return;
        
        var contact = other.GetContact(0);
            
        var inDir = moveDirection;   // 입사 방향
        var normal = contact.normal;           // 충돌면 법선

        var reflectDir = Vector2.Reflect(inDir, normal);

        moveDirection = reflectDir;
        rb.linearVelocity = moveDirection * moveSpeed;

    }
}
