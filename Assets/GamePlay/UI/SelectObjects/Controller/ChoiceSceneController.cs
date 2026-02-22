using GamePlay.UI.SelectObjects.CharacterChoice;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.UI.SelectObjects.Controller
{
    public class ChoiceSceneController : SelectObjectController
    {
        [SerializeField] private TMP_Text characterName;   
        [SerializeField] private TMP_Text characterInfo;
        [SerializeField] private CharacterSelect select;

        [SerializeField] private bool isChosen = false;

        [SerializeField] private SystemEnum.Character selectedCharacter;

        private InputAction cancelAction;
        
        protected override void Awake()
        {
            base.Awake();
            cancelAction = playerInput.actions["Cancel"];
        }

        protected override void Start()
        {
            base.Start();
            
            var characterObject = (CharacterChoiceObject)selectObjects[0];
            SettingCharacterInfoUI(characterObject.Character);
            
            
            
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            cancelAction.performed += OnCancelPressed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            cancelAction.performed -= OnCancelPressed;
        }

        private void ShowCharacterInfo()
        {
            // TODO: 대충 SO로 데이터 저장하는거 만들고 DataManager 만들고 거기서 캐릭터 정보 가져오면 됨
        }

        protected override void OnSelectPressed(InputAction.CallbackContext context)
        {
            if (isChosen) return;
            
            base.OnSelectPressed(context);
            var characterObject = (CharacterChoiceObject)selectObjects[currentIndex];
            SettingCharacterInfoUI(characterObject.Character);
        }

        protected override void OnDecidePressed(InputAction.CallbackContext context)
        {
            var choice = (CharacterChoiceObject)selectObjects[currentIndex];
            
            // 캐릭터가 선택되지 않았을 경우
            if (!isChosen)
            {
                if (choice.Character == SystemEnum.Character.None) return;
                base.OnDecidePressed(context);
                isChosen = true;
                selectedCharacter = choice.Character;
                select.Select();
            }
            else
            {
                GameManager.Instance.Scene.StartCharacterNovel(choice.Character, SystemEnum.NovelScriptType.before);
            }
        }

        private void OnCancelPressed(InputAction.CallbackContext context)
        {
            if (!isChosen) return;
            
            isChosen = false;
            selectedCharacter = SystemEnum.Character.None;
            select.Deselect();
        }

        private void SettingCharacterInfoUI(SystemEnum.Character character)
        {
            
            characterName.text = $"{currentIndex + 1}. {GameManager.Instance.Data.GetCharacterName(character, GameManager.Instance.language)}";
            characterInfo.text = $"{GameManager.Instance.Data.GetCharacterInfo(character, GameManager.Instance.language)}";
        }
    }
}
