using Managers;
using UnityEngine;

namespace GamePlay.UI.SelectObjects.GameOver
{
    public class ToMain : SelectObject
    {
        [SerializeField] private GameObject _canvas;
        public override void Execute()
        {
            // 메인 씬 로드
            _canvas.SetActive(false);
            GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Start);
        }
    }
}
