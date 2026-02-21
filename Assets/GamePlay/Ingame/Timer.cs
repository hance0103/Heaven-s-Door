using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Ingame
{
    public class Timer
    {
        private CancellationTokenSource cts;
        private bool isRunning;

        private float leftTime;

        public async UniTask StartTimerAsync(float seconds, Action<float> onTick = null, Action onComplete = null)
        {
            if (isRunning) return;

            isRunning = true;
            cts = new CancellationTokenSource();
            
            leftTime = seconds;

            try
            {
                while (leftTime > 0f)
                {
                    onTick?.Invoke(leftTime);
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                    leftTime -= Time.deltaTime;
                }

                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("타이머 중단됨");
            }
            finally
            {
                isRunning = false;
                cts.Dispose();
                cts = null;
            }
        }
        
        public void StopTimer()
        {
            if (!isRunning) return;

            cts.Cancel();
        }
    }
}
