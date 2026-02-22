using Cysharp.Threading.Tasks;
using GamePlay;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneLoadManager
    {
        public async UniTask Init()
        {
        
        }
        
        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void LoadScene(SystemEnum.eScenes scene)
        {
            SceneManager.LoadScene(scene.ToString());
        }
    
        // TODO : 뭐 나중에 캐릭터별로 이넘으로 다루던지 바꿀거임
        private string _novelName;
        public string NovelName;
    

        public void StartNovelScene(string novelName)
        {
            NovelName = novelName;
            LoadScene(SystemEnum.eScenes.Novel);
        }
    }
}
