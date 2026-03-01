using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadingUI : MonoBehaviour
{
    private static LoadingUI _instance;
    private static AsyncOperationHandle<GameObject>? _handle;

    public static async UniTask<LoadingUI> GetInstance()
    {
        if (_instance != null)
            return _instance;

        var handle = Addressables.InstantiateAsync("LoadingCanvas");
        _handle = handle;

        var go = await handle.Task;
        _instance = go.GetComponent<LoadingUI>();
        return _instance;
    }

    [SerializeField] private Transform loadingIcon;
    private Tween _rotateTween;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartRotate();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _rotateTween?.Kill();
    }

    private void StartRotate()
    {
        if (loadingIcon == null) return;

        _rotateTween = loadingIcon
            .DORotate(new Vector3(0, -360, 0), 1.2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void Release()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }

        if (_handle.HasValue && _handle.Value.IsValid())
        {
            Addressables.ReleaseInstance(_handle.Value);
            _handle = null;
        }
    }
}