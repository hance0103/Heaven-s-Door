using Managers;

namespace GamePlay.UI.SelectObjects.Start
{
    public class NewGame : SelectObject
    {
        public override void Execute()
        {
            GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Choice);
        }
    }
}
