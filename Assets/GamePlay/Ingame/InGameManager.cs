using System;
using GamePlay.GridMap;
using TMPro;
using UnityEngine;

namespace GamePlay.Ingame
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private TMP_Text scoreText;
        private int life = 3;
        

        private void Start()
        {
            life = 3;
        }

        public void RenewPercentage(int percent)
        {
            var percentage = percent;
            percentageText.text = $"{percentage.ToString()}%";
        }

        public void RenewScore()
        {
            var score = 0;
            scoreText.text = score.ToString();
        }
        
        [ContextMenu("라이프 하나 깎기")]
        public void MinusLife()
        {
            life -= 1;
            // 라이프 UI 오브젝트 하나 없애기
            // 죽는 모션
            // 부활 위치 선정
        }
        
        
        private void GameOver()
        {
            
        }
    }
}
