using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.UI.SelectObjects.GameOver
{
    public class ReTry : SelectObject
    {
        [SerializeField] private GameObject _canvas;
        public override void Execute()
        {
            // 일단은 현재 씬 재로드
            // TODO : 게임매니저에서 스테이지 로드하기
            _canvas.SetActive(false);
            
            GameManager.Instance.Scene.ReloadScene();
        }
    }
}
