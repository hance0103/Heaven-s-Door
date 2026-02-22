using Managers;

namespace GamePlay.UI.SelectObjects.Judge
{
    public class ToHeaven : SelectObject
    {
        public override void Execute()
        {
            GameManager.Instance.Scene.StartCharacterNovel(GameManager.Instance.currentCharacter, SystemEnum.NovelScriptType.heaven);
        }
    }
}
