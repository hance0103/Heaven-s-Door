using UnityEngine;

namespace GamePlay.UI.SelectObjects.CharacterChoice
{
    public class CharacterChoiceObject : SelectObject
    {
        [SerializeField] private SystemEnum.Character character;
        public SystemEnum.Character Character => character;

        public override void Execute()
        {
            // 해당하는 캐릭터 대화
            //GameManager.Instance.StartNovelScene("Karen_beforeText");
            // 캐릭터 정보 보여줌
        }
    
    }
}
