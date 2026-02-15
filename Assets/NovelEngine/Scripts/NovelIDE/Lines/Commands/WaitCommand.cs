using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class WaitCommand : CommandLine
    {
        float? waitTime;
        public WaitCommand(
            int index,
            float? waitTime, 
            IfParameter ifParameter = null) 
            : base(index, DialogoueType.CommandLine)
        {
            this.waitTime = waitTime;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;

            var player = NovelManager.Player;

            if (waitTime == null)
            {
                player.SetHardWait(true);
                player.SetWaitForTime(false);
                return;
            }


            // 일정 시간동안 멈춰두기 (취소 가능)

            float realTime = Mathf.Max(0f, waitTime.Value);
            player.SetHardWait(false);
            player.SetWaitForTime(true);

            try
            {
                await UniTask
                    .Delay(TimeSpan.FromSeconds(realTime),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update,
                    player.WaitToken);

            }
            catch (OperationCanceledException)
            {
                // 클릭으로 취소됨
                Debug.Log("Wait cancelled by click");

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

    }

}
