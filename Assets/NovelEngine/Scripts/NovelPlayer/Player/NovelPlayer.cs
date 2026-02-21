using Cysharp.Threading.Tasks;
using novel;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class NovelPlayer : MonoBehaviour
{
    [SerializeField] private Button skipButton;
    private TextAsset _novelScript;
    public NovelAct CurrentAct { get; private set; }
    public NovelEngine.Scripts.SerializableDict<string, int> LabelDict { get; private set; }
    [SerializeField] private bool isFinished = false;

    public bool isChoiceOn = false;
    // 자식 오브젝트들
    private GameObject _dialoguePanel;
    private GameObject _namePanel;
    private GameObject _blurPanel;
    private TextMeshProUGUI _novelText;
    private TextMeshProUGUI _nameText;
    private Button _nextButton;
    public GameObject BackgroundPanel { get; private set; }
    public GameObject StandingPanel { get; private set; }
    public GameObject ChoicePanel { get; private set; }
    
    public GameObject DialoguePanel => _dialoguePanel;

    [Header("Input")] 
    [SerializeField] private PlayerInput playerInput;
    private InputAction _nextScript;
    

    // 현재 배경화면
    public GameObject CurrentBackgroundObject { get; private set; }

    // 현재 스탠딩 나와 있는 캐릭터들
    public NovelEngine.Scripts.SerializableDict<NovelCharacterSO, GameObject> currentCharacterDict = new();
    //  현재 선택지
    public NovelEngine.Scripts.SerializableDict<ChoiceCommand, GameObject> currentChoices = new();

    private List<AsyncOperationHandle<GameObject>> backgroundHandles = new();
    private Dictionary<string, AsyncOperationHandle<GameObject>> standingHandles = new();

    // 이거 나중에 설정 가능하도록 바꾸기
    public float typingSpeed = 0.015f;
    private bool isTyping;

    // 실행시킬 서브라인
    private NovelLine currentSubline;
    private bool isSubLinePlaying;

    // 일정 시간동안 멈추는 기능
    private bool _isWaitForTime;
    // @wait 커맨드로 완전히 멈추기
    private bool _isHardWait;


    private bool isCommandRunning;  // 얘는 wait와는 다르게 진짜 명령어가 실행중이면 cancel 막기 위해서 필요


    private CancellationTokenSource _typingCts;
    private CancellationToken _destroyToken;

    private CancellationTokenSource _commandCts;
    private int _runningCommandCount; // 동시 커맨드 수

    private CancellationTokenSource _waitCts;
    //private bool _isWaitCmdRunning = false;

    private bool _isPumping;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
        Init();
        
        // 인풋 관련
        playerInput = GetComponent<PlayerInput>();
        _nextScript = playerInput.actions["NextScript"];
    }
    private void Init()
    {
        CurrentAct = new();
        _novelScript = null;
        FindObjectsByType();
        LabelDict = new();
    }

    private void OnEnable()
    {
        _nextScript.performed += OnNextLinePressed;
    }

    private void OnDisable()
    {
        _nextScript.performed -= OnNextLinePressed;
    }

    private void OnNextLinePressed(InputAction.CallbackContext context)
    {
        OnNextLineClicked();
    }

    private void FindObjectsByType()
    {
        var novelObjects = GetComponentsInChildren<NovelObjects>(true);
        foreach (var obj in novelObjects)
        {
            switch (obj.GetNovelObjectType())
            {
                case NovelObjectType.Blur:
                    _blurPanel = obj.gameObject;
                    break;
                case NovelObjectType.BackgroundPanel:
                    BackgroundPanel = obj.gameObject;
                    break;
                case NovelObjectType.StandingPanel:
                    StandingPanel = obj.gameObject;
                    break;
                case NovelObjectType.DialogPanel:
                    _dialoguePanel = obj.gameObject;
                    break;
                case NovelObjectType.NovelText:
                    _novelText = obj.GetComponent<TextMeshProUGUI>();
                    break;
                case NovelObjectType.NamePanel:
                    _namePanel = obj.gameObject;
                    break;
                case NovelObjectType.NameText:
                    _nameText = obj.GetComponent<TextMeshProUGUI>();
                    break;
                case NovelObjectType.NextButton:
                    _nextButton = obj.GetComponent<Button>();
                    break;
                case NovelObjectType.ChoicePanel:
                    ChoicePanel = obj.gameObject;
                    break;
            }
        }
    }
    public void SetScript(TextAsset text)
    {
        _novelScript = text;
    }
    public void Play()
    {
        // 플레이 해주기 전에 미리 novelScript 세팅해줘야함

        if (_novelScript == null)
        {
            Debug.LogError("노벨 스크립트가 설정되지 않음");
            return;
        }
        float t0 = Time.realtimeSinceStartup;
        var lines = _novelScript.text.Split('\n');
        

        CurrentAct = NovelParser.Parse(lines);

        if (CurrentAct == null)
        {
            Debug.Log("파싱 오류");
        }
        
        float ms = (Time.realtimeSinceStartup - t0) * 1000f;

        if (_nextButton != null)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(OnNextLineClicked);
        }


        CurrentAct?.ResetAct();

        _dialoguePanel.SetActive(false);
        // 이거 시작 시점 언젠지 상의 필요
        OnNextLineClicked();
    }
    private void OnNextLineClicked()
    {
        // Act가 끝났거나 wait 상태
        if (isFinished || _isHardWait) return;



        if (isCommandRunning)
        {
            CancelCommandsAsync();
        }

        // 시간이 정해진 @wait 커맨드
        if (_isWaitForTime) return;




        // 여기서부터는 정상적으로 라인 실행

        // 텍스트 타이핑 중이면 바로 전체 출력
        if (isTyping)
        {
            _typingCts?.Cancel();
            return;
        }

        if (!_isPumping)
        {
            _isPumping = true;
            NextLine().Forget();
        }
    }  
    public void EndScript()
    {
        // 여기가 스크립트 끝났을때 콜백
        NovelManager.Instance.OnScriptEnd();
        NovelManager.Instance.Audio.StopBGM();

        ReleaseObjects();
        
        Addressables.ReleaseInstance(gameObject);
        

        
    }

    private async UniTask NextLine()
    {
        bool keepPumping = false;
        try
        {
            var result = await ProcessLine();
            switch (result)
            {
                case LineResult.Finished:
                    EndScript();

                    return;
                case LineResult.Stop:
                    return;
                case LineResult.Continue:
                    {
                        keepPumping = true;
                        NextLine().Forget();
                        break;
                    }
            }
        }
        finally
        {
            if (!keepPumping)
                _isPumping = false;
        }
    }
    private enum LineResult { Continue, Stop, Finished }

    private async UniTask<LineResult> ProcessLine()
    {
        if (_isHardWait) return LineResult.Stop;

        if (isSubLinePlaying && currentSubline is CommandLine subCommand)
        {
            RunCommandLine(subCommand);
            isSubLinePlaying = false;
            return LineResult.Continue;
        }

        var line = CurrentAct.GetNextLine();

        if (line == null)
        {
            Debug.Log("스크립트 끝까지 플레이");
            return LineResult.Finished;
        }

        switch (line)
        {
            case LabelLine:
                return LineResult.Continue;
            case CommandLine command:
                if (command is WaitCommand wait)
                {
                    RunCommandLine(wait);
                    return LineResult.Stop;
                }
                RunCommandLine(command);
                return LineResult.Continue;
        }
        PlayLine(line);
        return LineResult.Stop;
    }
    private void RunCommandLine(CommandLine command)
    {
        if (command is WaitCommand wait)
        {
            OnWaitStart();
            RunWaitCommandAsync(wait).Forget();
        }
        else
        {
            OnCommandStart();
            RunCommandLineAsync(command).Forget();
        }
    }
    private async UniTaskVoid RunCommandLineAsync(CommandLine command)
    {
        try
        {
            await command.Execute();
        }
        catch (OperationCanceledException)  
        {
            // 여기서 캔슬되는것은 연출 관련 UniTask만 캔슬됨 - 커맨드의 기본 동작은 캔슬되지 않음
            Debug.Log("Command Cancelled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Command Execution Error: {ex.Message}");
        }
        finally
        {
            OnCommandEnd();
        }
    }
    private async UniTaskVoid RunWaitCommandAsync(WaitCommand wait)
    {
        try
        {
            await wait.Execute();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally 
        {
            //Debug.Log("wait 종료");
            OnWaitEnd();
        }
    }
    private void OnCommandStart()
    {
        _runningCommandCount++;

        // 가장 처음 실행되는 커맨드에 대해서만 취소 토큰 생성
        if (_runningCommandCount == 1)
        {
            isCommandRunning = true;
            _commandCts?.Cancel();
            _commandCts?.Dispose();
            _commandCts = new CancellationTokenSource();
        }
    }
    private void OnCommandEnd()
    {
        _runningCommandCount = Math.Max(0, _runningCommandCount - 1);
        if (_runningCommandCount == 0)
        {
            isCommandRunning = false;
            _commandCts?.Dispose();
            _commandCts = null;
        }
    }

    private void OnWaitStart()
    {
        _waitCts?.Cancel();
        _waitCts?.Dispose();
        _waitCts = new();
    }

    private async void OnWaitEnd()
    {
        await UniTask.WaitUntil(() => _runningCommandCount == 0, cancellationToken: _destroyToken);
        _waitCts?.Dispose();
        _waitCts = null;
        _isWaitForTime = false;
        OnNextLineClicked();
    }
    private void CancelCommandsAsync()
    {
        _commandCts?.Cancel();
        _waitCts?.Cancel();
    }

    public void SetSublinePlaying(NovelLine line)
    {
        isSubLinePlaying = true;
        currentSubline = line;
    }
    public void SetHardWait (bool value)
    {
        _isHardWait = value;
    }
    public void SetWaitForTime(bool value)
    {
        _isWaitForTime = value;
    }

    public void ContinueFromWait() => OnNextLineClicked();
    public void Resume()
    {
        _isWaitForTime = false;
        _isHardWait = false;
        OnNextLineClicked();
    }

    public void SetBackground(GameObject background)
    {
        CurrentBackgroundObject = background;
    }
    public CancellationToken CommandToken
        =>_commandCts?.Token ?? CancellationToken.None;
    public CancellationToken WaitToken
        => _waitCts?.Token ?? CancellationToken.None;


    // 텍스트 패널에 있는 텍스트들 플레이 해주는 함수
    private void PlayLine(NovelLine line)
    {
        _dialoguePanel.SetActive(true);

        _typingCts?.Cancel();
        _typingCts?.Dispose();
        _typingCts = CancellationTokenSource.CreateLinkedTokenSource(_destroyToken);

        switch (line)
        {
            case NormalLine normal:
                // 독백시 모든 스탠딩 캐릭터 어둡게
                foreach (GameObject standingObject in currentCharacterDict.values)
                {
                    Transform transform =  standingObject.transform;
                    foreach (var img in transform.GetComponentsInChildren<Image>())
                    {

                        img.color = new Color(100/255f, 100/255f, 100/255f);
                    }
                }
                
                
                _namePanel.SetActive(false);
                _ = TypeTextAsync(normal.line, _typingCts.Token);
                //Debug.Log($"Play Normal Line :  {normal.line} \nIndex : {normal.index}");
                break;
            case PersonLine person:
                
                _namePanel.SetActive(true);
                _nameText.text = person.actorName;
                _ = TypeTextAsync(person.actorLine, _typingCts.Token);
                
                
                // TODO : SerialzableDict Enumerable로 바꿔
                // 현재 화자가 아닌 스탠딩은 전부 어둡게 처리
                
                foreach (NovelCharacterSO standing in currentCharacterDict.Keys)
                {
                    // 말하는 사람만 하이라이트
                    if (person.actorName == standing.novelName)
                    {
                        Transform obj = currentCharacterDict[standing].transform;
                        foreach (var img in obj.GetComponentsInChildren<Image>())
                        {
                            img.color = Color.white;
                        }
                    }
                    else
                    {
                        Transform transform = currentCharacterDict[standing].transform;
                        foreach (var img in transform.GetComponentsInChildren<Image>())
                        {

                            img.color = new Color(100/255f, 100/255f, 100/255f);
                        }
                    }
                }
                

                //Debug.Log($"Play Person Line :  {person.actorLine} \nIndex : {person.index}");
                break;
        }
    }
    // 타이핑 하는 연출
    private async UniTask TypeTextAsync(string fullText, CancellationToken token)
    {
        isTyping = true;
        _novelText.text = "";
        if (typingSpeed <= 0f)
        {
            _novelText.text = fullText;
            isTyping = false;
            return;
        }
        StringBuilder stringBuilder = new StringBuilder(fullText.Length);

        foreach (char t in fullText.TakeWhile(t => !token.IsCancellationRequested))
        {
            stringBuilder.Append(t);
            _novelText.text = stringBuilder.ToString();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(typingSpeed), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        // 스킵/완료 모두 최종 전체 출력 보장
        _novelText.text = fullText;
        isTyping = false;
    }
    
    private void OnDestroy()
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
    }

    public void OnClickSKipButton()
    {
        if (isChoiceOn)
            return;
        
        for (int i = CurrentAct.CurrentIndex; i < CurrentAct.novelLines.Count + 1; i++)
        {
            NovelLine line = CurrentAct.GetLineFromIndex(i);
            if (line == null)
                return;
            
            if (line is not CommandLine command)
            {
                continue;
            }
            
            
            switch (command)
            {
                case GotoCommand:
                    //Debug.Log("Goto 실행");
                    command.Execute();
                    i = CurrentAct.CurrentIndex + 1;
                    break;
                case ChoiceCommand:
                    Debug.Log($"선택지 도달 Index : {i}");
                    CurrentAct.JumpToIndex(i);
                    OnNextLineClicked();
                    ReleaseStandings();
                    
                    return;
            }


            // if (command is GotoCommand)
            // {
            //     Debug.Log("Goto 실행");
            //     command.Execute();
            //     i = CurrentAct.CurrentIndex - 1;
            //
            // }
            // else if (command is ChoiceCommand)
            // {
            //     Debug.Log("선택지 도달");
            //     CurrentAct.JumpToIndex(i);
            //     OnNextLineClicked();
            //     return;
            // }
        }
        Debug.Log("선택지 없음 플레이 종료");
        
        EndScript();
    }

    public void ReleaseObjects()
    {
        ReleaseBackground();
        ReleaseStandings();
        ReleaseChoices();
    }

    private void ReleaseBackground()
    {
        foreach (Transform child in BackgroundPanel.transform)
        {
            Addressables.Release(child.gameObject);
        }
    }
    private void ReleaseStandings()
    {
        foreach (GameObject standingObject in currentCharacterDict.Values)
        {
            Addressables.Release(standingObject);
        }
        currentCharacterDict.Clear();
    }

    private void ReleaseChoices()
    {
        foreach (GameObject choiceObjects in currentChoices.Values)
        {
            Addressables.Release(choiceObjects);
        }
        currentChoices.Clear();
    }
    
    
}