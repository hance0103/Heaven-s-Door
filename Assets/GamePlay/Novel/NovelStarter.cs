using System;
using GamePlay;
using UnityEngine;

public class NovelStarter : MonoBehaviour
{
    void Start()
    {
        PlayScript();
    }
    private async void PlayScript()
    {
        try
        {
            await NovelManager.InitAsync();
            await NovelManager.Instance.PlayScript(GameManager.Instance.NovelName);
            
            NovelManager.Instance.OnScriptEndEvent += OnScriptEnd;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
        
    }

    private void OnDisable()
    {
        NovelManager.Instance.OnScriptEndEvent -= OnScriptEnd;
    }

    private void OnScriptEnd()
    {
        // TODO: 어떤 캐릭터 시작 스토리였는지 구분해서 해당하는 스테이지 옮길것
        GameManager.Instance.LoadScene(SystemEnum.eScenes.Ingame);
    }
}
