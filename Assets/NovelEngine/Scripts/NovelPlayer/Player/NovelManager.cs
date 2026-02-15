using novel;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.PlayerLoop;

public class NovelManager : MonoBehaviour
{
    //// TODO 씨이이발 임시코드
    //public bool isRewardOpen = false;
    
    
    public static NovelManager Instance
    {
        get => instance;
        set => instance = value;
    }
    private static NovelManager instance;
    public static NovelResources Data { get; private set; } = new NovelResources();

    public NovelAudioManager Audio { get; private set; } = new();

    private const string NovelPlayerPrefabPath = "NovelPlayer";
    public static NovelPlayer Player { get; private set; }

    private static readonly SemaphoreSlim NovelPlayerGate = new SemaphoreSlim(1, 1);

    public UniTask Initialization => _initialization;
    private static bool isReady;

    UniTask _initialization = UniTask.CompletedTask;
    static bool initStarted;
    /// <summary>
    /// 튜토리얼용 임시 함수
    /// </summary>
    /// <param name="index"></param>
    public void PlayTutorial(int index)
    {
        string name = $"Tutorial_{index}";
        Debug.Log($"PlayTutorial :  {name}");
        PlayScript(name);
    }

    //public bool firstTutoEnd = false;
    //public bool secondTutoEnd = false;
    //public bool thirdTutoEnd = false;
    //public bool fourthTutoEnd = false;
    
    public static async void Init()
    {
        Debug.Log("Init");
        if (instance == null)
        {
            // NovelManager가 이미 씬에 존재하는지 확인
            var existing = FindObjectOfType<NovelManager>();
            // 있으면 그걸 사용, 없으면 새로 생성
            Instance = existing ?? new GameObject("@Novel").AddComponent<NovelManager>();
        }

        // 메인 스레드에서 초기화 해야함
        await Instance.InitializeIfNeededAsync();

    }

    public static async UniTask<NovelManager> InitAsync()
    {
        Debug.Log("Init Async");
        if (instance == null)
        {
            // NovelManager가 이미 씬에 존재하는지 확인
            var existing = FindObjectOfType<NovelManager>();
            // 있으면 그걸 사용, 없으면 새로 생성
            Instance = existing ?? new GameObject("@Novel").AddComponent<NovelManager>();
        }
        await Instance.InitializeIfNeededAsync();

        if (initStarted)
        {
            // 초기화가 시작되었을 경우
            while (!isReady)
            {
                // 완료될때까지 대기
                await UniTask.Yield();
            }
        }
        
        return Instance;
    }
    void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance ??= this;

        // 게임 오브젝트 이름 강제
        gameObject.name = "@Novel";

        // 씬 전환시 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }
    private UniTask InitializeIfNeededAsync()
    {
        if(isReady)
            return UniTask.CompletedTask;
        
        // 이미 초기화가 시작되었으면 기존 Task 반환
        if (initStarted)
        {
            Debug.Log("[NovelManager] Initialization already started, returning existing task.");
            return _initialization;
        }


        // 초기화 시작
        initStarted = true;
        _initialization = InitializeAsync().Preserve();

        return _initialization;
    }

    // 여기가 진짜 초기화 코드
    private async UniTask InitializeAsync()
    {
        // 라벨로 SO 전체 로드
        await Data.InitByLabelAsync();
  
        // 오디오매니저 초기화
        await Audio.AudioManagerInitAsync();

        isReady = true;
        Debug.Log("Novel Engine 초기화 완료");
    }
    public static async UniTask ShutdownAsync()
    {
        if (Instance == null) return;

        try { Data.OnNovelEnd(); } catch { /* ignore */ }

        if (Player != null)
        {
            Addressables.ReleaseInstance(Player.gameObject);
            Player = null;
        }

        var go = Instance.gameObject;
        Instance = null;
        if (go != null) Destroy(go);

        
        await UniTask.Yield();
    }

    public async UniTask PlayScript(string scriptTitle)
    {
        if (instance == null)
        {
            await NovelManager.InitAsync();
        }
        PlayScriptAsync(scriptTitle);
    }
    private async UniTask PlayScriptAsync(string scriptTitle)
    {
        try
        {
            if (!isReady)
            {
                Debug.LogError("[NovelManager] Not ready yet. Call InitializeAsync() first.");
                return;
            }
            // 노벨 플레이어 인스턴스화 시켜줌
            if (Player == null)
                await InstantiateNovelPlayerAsync();

            // 원하는 스크립트 장착
            TextAsset script = Data.script.GetScriptByTitle(scriptTitle);
            
            // 여기까진 일단 정상작동
            if (script == null)
            {
                Debug.LogError($"[NovelManager] Script '{scriptTitle}' not found.");
                return;
            }
            
            Debug.Log(Player);
            
            
            Player.SetScript(script);
            
            Debug.Log("플레이 함수 실행");
            
            // 플레이 해줌
            Player.Play();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
    
    /// <summary>
    /// Novel 끝나는거까지 기다리는 함수 팜.
    /// TODO : 설계한 쪽에서 검수 바람
    /// </summary>
    public static async UniTask PlayScriptAndWait(string scriptTitle, CancellationToken ct = default)
    {
        if (!instance)
        {
            await InitAsync();
        }

        if (!isReady)
        {
            Debug.LogError("[NovelManager] Not ready yet. Call InitializeAsync() first.");
            return;
        }

        if (!Player)
            await instance.InstantiateNovelPlayerAsync();

        TextAsset script = Data.script.GetScriptByTitle(scriptTitle);
        if (!script)
        {
            Debug.LogError($"[NovelManager] Script '{scriptTitle}' not found.");
            return;
        }

        var tcs = new UniTaskCompletionSource();

        void Handler()
        {
            Player.OnScriptEnd -= Handler;
            tcs.TrySetResult();
        }

        Player.OnScriptEnd += Handler;

        if (ct.CanBeCanceled)
        {
            ct.Register(() =>
            {
                Player.OnScriptEnd -= Handler;
                tcs.TrySetCanceled(ct);
            });
        }

        Player.SetScript(script);
        Player.Play();

        await tcs.Task;
        
    }
    
    
    private async UniTask InstantiateNovelPlayerAsync()
    {
        await NovelPlayerGate.WaitAsync();
        try
        {
            // 가장 먼저 NovelPlayer 컴포넌트가 자식 오브젝트에 있는지 확인
            if (Player == null)
                Player = GetComponentInChildren<NovelPlayer>(true);
            else
            {
                Debug.Log(" [NovelManager] NovelPlayer already exists.");
                return;
            }


            // 없으면 Addressable에서 로드 및 인스턴스화
            if (Player == null)
            {
                GameObject go = await Addressables.InstantiateAsync("NovelPlayer", transform);
                

                
                if (go != null)
                {
                    go.name = "NovelPlayer";
                    Player = go.GetComponent<NovelPlayer>();

                }
                else
                {
                    Debug.LogError($"[NovelManager] Failed to load prefab: {NovelPlayerPrefabPath}");
                }
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NovelManager] Error while checking for existing NovelPlayer: {ex.Message}");
            return;
        }
        finally
        {
            
            NovelPlayerGate.Release();
        }
        Debug.Log(" [NovelManager] NovelPlayer instantiated.");
    }

    public void ReleaseNovelPlayer()
    {
        Addressables.ReleaseInstance(Player.gameObject);
    }
}