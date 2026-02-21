namespace GamePlay.UI.SelectObjects.Start
{
    public class NewGame : SelectObject
    {
        public override void Execute()
        {
            GameManager.Instance.LoadScene(SystemEnum.eScenes.Choice);
        }
    }
}
