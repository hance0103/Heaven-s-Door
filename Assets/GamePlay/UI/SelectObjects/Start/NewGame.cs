using Managers;

namespace GamePlay.UI.SelectObjects.Start
{
    public class NewGame : SelectObject
    {
        public override void Execute()
        {
            GameManager.Instance.scriptType = SystemEnum.NovelScriptType.Prolog;
            GameManager.Instance.Scene.StartNovelScene("prologue");
            //GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Choice);
        }
    }
}
