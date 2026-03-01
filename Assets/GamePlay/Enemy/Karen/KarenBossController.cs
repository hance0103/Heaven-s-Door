using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Enemy;
using GamePlay.Enemy.Karen.Skills;
using Managers;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class KarenBossController : EnemyController
{
    [SerializeField] private string skillUsePattern;
    private int currentSkillIndex;
    [SerializeField] private float firstStopTime;
    [SerializeField] private Vector2 skillUseCoolTime;
    [SerializeField] private KarenSkill1 skill1;
    [SerializeField] private KarenSkill2 skill2;
    [SerializeField] private KarenSkill3 skill3;

    private bool canUseSkill = true;
    
    protected override async void Start()
    {
        try
        {
            base.Start();
            await UniTask.Delay(TimeSpan.FromSeconds(firstStopTime));
            
            currentSkillIndex = UnityEngine.Random.Range(0, skillUsePattern.Length);
            
            
            SetNextState(bossState);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public override void SetNextState(BossState state)
    {
        var ingameManager = GameManager.Instance.inGameManager;
        if (ingameManager == null || ingameManager.IsGameEnd) return;
        
        switch (bossState)
        {
            case BossState.Idle:
            {
                _ = UseSkill(this.GetCancellationTokenOnDestroy());
                break;
            }
            case BossState.Move:
            {
                _ = UseSkill(this.GetCancellationTokenOnDestroy());
                break;
            }
            case BossState.Attack:
            {
                
                MoveToPlayer();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private async UniTask UseSkill(CancellationToken token = default)
    {
        var ingame = GameManager.Instance.inGameManager;
        if (ingame == null || ingame.IsGameEnd) return;
        
        try
        {
            bossState = BossState.Attack;

            switch (skillUsePattern[currentSkillIndex] - '0')
            {
                case 1:
                {
                    await skill1.UseSKill(token);
                    break;
                }
                case 2:
                {
                    await skill2.UseSKill(token);
                    break;
                }
                case 3:
                {
                    await skill3.UseSKill(token);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {

            currentSkillIndex++;
            if (currentSkillIndex >= skillUsePattern.Length)
                currentSkillIndex = 0;
            // 최종적으로 다음 state로 진행
            SetNextState(bossState);
            _ = SkillUseCool(token);
        }
    }

    private async UniTask SkillUseCool(CancellationToken token = default)
    {
        canUseSkill = false;
        var coolTime = UnityEngine.Random.Range(skillUseCoolTime.x, skillUseCoolTime.y);
        await UniTask.Delay(TimeSpan.FromSeconds(coolTime), cancellationToken: token);
        canUseSkill = true;
    }
    
    protected override async void MoveToPlayer()
    {
        base.MoveToPlayer();

        await UniTask.WaitUntil(() => canUseSkill);
        
        Rigidbody.linearVelocity = Vector3.zero;
        SetNextState(bossState);
    }

    public void StateCheck()
    {
        
    }
    
}
