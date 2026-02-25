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

    [Header("스킬 1")] [SerializeField] private KarenSkill1 skill1;
    
    protected override void Start()
    {
        base.Start();

        SetNextState(bossState);
    }

    public override void SetNextState(BossState state)
    {
        if (GameManager.Instance.inGameManager.IsGameEnd) return;
        
        switch (bossState)
        {
            case BossState.Idle:
            {
                UseSkill();
                break;
            }
            case BossState.Move:
                break;
            case BossState.Attack:
            {
                // 공격 후 어떤 동작할지 정하기
                OnIdle();
                break;
            }
            case BossState.Wait:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void OnIdle()
    {
        try
        {
            bossState = BossState.Idle;

            await UniTask.Delay(1000);


        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            SetNextState(bossState);
        }
    }
    private async void UseSkill(CancellationToken token = default)
    {
        try
        {
            bossState = BossState.Attack;

            // 어떤 스킬 사용할지 정하는 코드
            // TODO: 일단 스킬1만 박아둠
            await skill1.UseSKill(token);


        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            // 최종적으로 다음 state로 진행
            SetNextState(bossState);
        }
    }
    public void StateCheck()
    {
        
    }
    
}
