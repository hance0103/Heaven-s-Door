using System;
using GamePlay;
using GamePlay.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    #region Singleton

    // instance 멤버변수는 private하게 선언
    private static GameManager _instance = null;

    public static bool IsGamePlaying = false;
    
    private void Awake()
    {
        if (null == _instance)
        {
            // 씬 시작될때 인스턴스 초기화, 씬을 넘어갈때도 유지되기위한 처리
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // instance가, GameManager가 존재한다면 GameObject 제거 
            Destroy(this.gameObject);
        }
    }
    
    // Public 프로퍼티로 선언해서 외부에서 private 멤버변수에 접근만 가능하게 구현
    public static GameManager Instance
    {
        get
        {
            if (null != _instance) return _instance;
            
            // 씬에서 먼저 찾아봄
            _instance = FindFirstObjectByType<GameManager>();

            if (_instance != null) return _instance;
                
                
            // 없으면 새로 생성
            var go = new GameObject("@GameManager");
            _instance = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    #endregion
    
    public PlayerController playerController;
    
    
    // TODO : 이것들 나중에 딴데로 옮길것
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
