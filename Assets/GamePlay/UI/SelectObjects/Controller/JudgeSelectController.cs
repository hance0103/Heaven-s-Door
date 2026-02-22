using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.AddressableAssets.Addressables;

namespace GamePlay.UI.SelectObjects.Controller
{
    public class JudgeSelectController : SelectObjectController
    {
        [SerializeField] private Image standing;
        [SerializeField] private TMP_Text charName;
        [SerializeField] private GameObject judgeCanvas;
        
        protected override void Start()
        {
            base.Start();
            SettingJudgeUI();
        }

        private void SettingJudgeUI()
        {
            // 캐릭터 스탠딩 일러스트 변경
            charName.text =
                GameManager.Instance.Data.GetCharacterName(SystemEnum.Character.Karen, SystemEnum.Language.KOR);
        }

        protected override void OnDecidePressed(InputAction.CallbackContext context)
        {
            base.OnDecidePressed(context);
            ReleaseInstance(judgeCanvas);
        }
    }
}
