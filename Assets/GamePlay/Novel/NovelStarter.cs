using System;
using Cysharp.Threading.Tasks;
using GamePlay;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NovelStarter : MonoBehaviour
{
    private bool _endingHandled;
    void Start()
    {
        PlayScript();
    }
    private async void PlayScript()
    {
        try
        {
            await NovelManager.InitAsync();

            
            NovelManager.Instance.OnScriptEndEvent -= OnScriptEnd;
            NovelManager.Instance.OnScriptEndEvent += OnScriptEnd;
            
            await NovelManager.Instance.PlayScript(GameManager.Instance.Scene.NovelName);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
        
    }
    private void OnDisable()
    {
        if (NovelManager.Instance != null)
            NovelManager.Instance.OnScriptEndEvent -= OnScriptEnd;
    }
    
    private void OnScriptEnd()
    {
        if (_endingHandled) return;
        _endingHandled = true;

        HandleScriptEndAsync().Forget();
    }
    
    private async UniTaskVoid HandleScriptEndAsync()
    {
        switch (GameManager.Instance.scriptType)
        {
            case SystemEnum.NovelScriptType.before:
                GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Ingame);
                break;

            case SystemEnum.NovelScriptType.after:
                try
                {
                    var prefab = await Addressables.LoadAssetAsync<GameObject>("JudgeCanvas");
                    Instantiate(prefab);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;

            case SystemEnum.NovelScriptType.heaven:
            case SystemEnum.NovelScriptType.hell:
            case SystemEnum.NovelScriptType.Prolog:
                GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Choice);
                break;

            case SystemEnum.NovelScriptType.None:
            default:
                Debug.LogError($"Invalid scriptType: {GameManager.Instance.scriptType}");
                break;
        }
    }
    

}
