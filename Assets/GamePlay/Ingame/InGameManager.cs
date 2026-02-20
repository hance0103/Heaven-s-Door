using System;
using Cysharp.Threading.Tasks;
using GamePlay.GridMap;
using GamePlay.Player;
using TMPro;
using UnityEngine;

namespace GamePlay.Ingame
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private int life = 3;
        private bool isDying = false;
        
        [SerializeField] private GameObject gameOverPanel;
        
        
        private void Start()
        {
            life = 3;
            isDying = false;
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
        public async void MinusLife()
        {
            if (isDying) return;
            isDying = true;

            try
            {
                var player = GameManager.Instance.playerController;
                if (player == null) return;
                
                // TODO : 죽는 모션
                
                life--;
                // TODO : 라이프 깎기 (UI)
                
                player.SetCanMove(false);
                
                if (life <= 0)
                {
                    GameOver(player.gameObject);
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
        
        
        private void GameOver(GameObject player)
        {
            player.SetActive(false);
            gameOverPanel.SetActive(true);
        }
    }
}
