using System;
using Cysharp.Threading.Tasks;
using GamePlay;
using Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            await NovelManager.Instance.PlayScript(GameManager.Instance.Scene.NovelName);
            
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

    private async void OnScriptEnd()
    {
        // TODO: 어떤 캐릭터의 스테이지인지는 IngameScene 시작할때 설정해줄것
        

        switch (GameManager.Instance.scriptType)
        {
            case SystemEnum.NovelScriptType.before:
            {
                GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Ingame);
                break;
            }
            case SystemEnum.NovelScriptType.after:
            {
                try
                {
                    var canvasPrefab =
                        await Addressables.LoadAssetAsync<GameObject>("JudgeCanvas");

                    Instantiate(canvasPrefab);
                    
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }

                break;
            }
            case SystemEnum.NovelScriptType.heaven:
            case SystemEnum.NovelScriptType.hell:
            {
                GameManager.Instance.Scene.LoadScene(SystemEnum.eScenes.Choice);
                break;
            }
            case SystemEnum.NovelScriptType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
        

    }
}
