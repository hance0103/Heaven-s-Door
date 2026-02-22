using Managers;

namespace GamePlay.UI.SelectObjects.GameOver
{
    public class ToMain : SelectObject
    {
        public override void Execute()
        {
            // 메인 씬 로드
            GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Start);
        }
    }
}
