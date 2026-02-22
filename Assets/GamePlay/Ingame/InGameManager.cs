using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using TMPro;
using UnityEngine;

namespace GamePlay.Ingame
{
    public class InGameManager : MonoBehaviour
    {
        [Header("게임 시간")]
        [SerializeField] private int gameTime = 0;

        [Header("게임 승리 조건")] 
        [Tooltip("일반 승리")]
        [SerializeField] private int normalPercentage = 80;
        [Tooltip("쇼타임 승리")]
        [SerializeField] private int showtimePercentage = 100;
        [Header("타이머 흔들리는 각도")] 
        [SerializeField] private float shakeAngle = 10;
        
        [Header("텍스트 오브젝트")]
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timerText;
        
        [SerializeField] private int life = 3;
        [SerializeField] private List<GameObject> lifeObjects = new();
        
        private bool isDying = false;
        private bool isGameEnd = false;
        [SerializeField] private GameObject gameOverPanel;
        
        private int totalScore = 0;

        private Timer timer;
        private bool isTimerShakeStart = false;
        private Tween shakeTween;

        private void Start()
        {
            life = 3;
            totalScore = 0;
            isDying = false;
            
            RenewPercentage(0);
            
            timer = new Timer();
            _ = timer.StartTimerAsync(gameTime,
            RenewTimer,
            GameOver);
        }


        private void RenewTimer(float leftTime)
        {
            string leftTimeString;
            
            if (leftTime <= 10)
            {
                leftTimeString = leftTime.ToString("F2");
                if (!isTimerShakeStart)
                {
                    isTimerShakeStart = true;
                    StartTimerShake();

                }
            }
            else
            {
                var leftTimeInt = (int)leftTime;
                leftTimeString = leftTimeInt.ToString();
            }

            
            timerText.text = leftTimeString;
        }

        private void StartTimerShake()
        {
            shakeTween?.Kill();
            

            // 시작 각도를 왼쪽으로 미리 설정
            timerText.transform.localRotation = Quaternion.Euler(0, 0, -shakeAngle);

            shakeTween = timerText.transform
                .DOLocalRotate(
                    new Vector3(0, 0, shakeAngle),
                    0.1f
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        private void StopShake()
        {
            shakeTween?.Kill();
            timerText.transform.localRotation = Quaternion.identity;
        }

        
        public void RenewPercentage(int percent)
        {
            percentageText.text = $"{percent.ToString()}%";
            JudgeGameEnd(percent);
        }

        public void IncreaseScore(int increasedPercentage)
        {

            var score = 0;
            
            switch (increasedPercentage)
            {
                case >= 0 and < 1:
                    // 0점 증가
                    break;
                case >= 1 and < 3:
                    // 5점 증가
                    score = 5;
                    break;
                case >= 3 and < 5:
                    // 100점 증가
                    score = 100;
                    break;
                case >= 5 and < 10:
                    // 200점 증가
                    score = 200;
                    break;
                default:
                    // 450점
                    score = 450;
                    break;
            }
            
            totalScore += score;
            scoreText.text = totalScore.ToString();
        }
        
        [ContextMenu("라이프 하나 깎기")]
        public async void MinusLife()
        {
            if (isDying) return;
            isDying = true;

            var player = GameManager.Instance.playerController;
            
            try
            {
                if (player == null) return;
                
                // TODO : 죽는 모션
                
                life--;
                // TODO : 라이프 깎기 (UI)
                lifeObjects[life].SetActive(false);
                
                player.SetCanMove(false);
                
                if (life <= 0)
                {
                    GameOver();
                    return;
                }
                
                player.SetPositionWhenRevive();
                player.SetCanMove(true);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                isDying = false;
            }
        }
        
        private void GameOver()
        {
            if (isGameEnd) return;
            isGameEnd = true;
            
            timer.StopTimer();
            StopShake();

            GameManager.Instance.playerController.gameObject.SetActive(false);
            gameOverPanel.SetActive(true);
        }

        private void JudgeGameEnd(int percent)
        {
            if (percent >= normalPercentage && percent < showtimePercentage)
            {
                NormalGameWin();
            }
            else if (percent >= showtimePercentage)
            {
                ShowTimeWin();
            }
        }
        
        // 게임 승리시 공통적으로 해야할 작업
        private void GameWin()
        {
            Debug.Log("게임 승리");
            
            if (isGameEnd) return;
            isGameEnd = true;
            
            timer.StopTimer();
            StopShake();
            GameManager.Instance.playerController.SetCanMove(false);
        }
        
        private void NormalGameWin()
        {
            GameWin();
        }

        private void ShowTimeWin()
        {
            GameWin();
        }
    }
}
