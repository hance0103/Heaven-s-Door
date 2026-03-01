using System;
using UnityEngine;

public class NovelTest : MonoBehaviour
{
    public string scriptName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlayTutorial_1();
    }
    private async void PlayTutorial_1()
    {
        try
        {
            await NovelManager.InitAsync();
            await NovelManager.Instance.PlayScript(scriptName);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

}
