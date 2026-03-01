using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using GamePlay;

namespace Managers
{
    public class SceneLoadManager
    {

        public void ReloadScene()
        {
            LoadSceneAsync(SceneManager.GetActiveScene().buildIndex).Forget();
        }

        public void LoadScene(SystemEnum.eScenes scene)
        {
            LoadSceneAsync(scene.ToString()).Forget();
        }

        public string NovelName { get; private set; }

        public void StartNovelScene(string novelName)
        {
            NovelName = novelName;
            LoadScene(SystemEnum.eScenes.Novel);
        }

        public void StartCharacterNovel(SystemEnum.Character charName, SystemEnum.NovelScriptType scriptType)
        {
            GameManager.Instance.currentCharacter = charName;
            GameManager.Instance.scriptType = scriptType;
            StartNovelScene($"{charName}_{scriptType}Text");
        }

        private async UniTask LoadSceneAsync(int buildIndex)
        {
            await LoadSceneInternal(() => SceneManager.LoadSceneAsync(buildIndex));
        }

        private async UniTask LoadSceneAsync(string sceneName)
        {
            await LoadSceneInternal(() => SceneManager.LoadSceneAsync(sceneName));
        }

        private async UniTask LoadSceneInternal(Func<AsyncOperation> loadFunc)
        {
            var loadingUI = await LoadingUI.GetInstance();
            loadingUI.Show();

            var startTime = Time.realtimeSinceStartup;

            var op = loadFunc();
            op.allowSceneActivation = false;

            await UniTask.WaitUntil(() => op.progress >= 0.9f);

            var elapsed = Time.realtimeSinceStartup - startTime;
            var remain = loadingUI.MinLoadingTime - elapsed;

            if (remain > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(remain));

            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);

            loadingUI.Hide();
        }
    }
}