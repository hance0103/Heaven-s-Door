using GamePlay.Player;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    // instance 멤버변수는 private하게 선언
    private static GameManager _instance = null;

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
            var go = new GameObject("GameManager");
            _instance = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    #endregion
    
    public PlayerController playerController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
