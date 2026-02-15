using UnityEngine;

public class NovelTest : MonoBehaviour
{
    public string scriptName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayTutorial_1();
    }
    private async void PlayTutorial_1()
    {
        await NovelManager.InitAsync();
        NovelManager.Instance.PlayScript(scriptName);

    }

}
