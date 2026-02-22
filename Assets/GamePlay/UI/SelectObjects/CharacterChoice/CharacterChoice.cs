using UnityEngine;

namespace GamePlay.UI.SelectObjects.CharacterChoice
{
    public class CharacterChoiceObject : SelectObject
    {
        [SerializeField] private SystemEnum.Character character;
        public SystemEnum.Character Character => character;

        public override void Execute()
        {
        }
    
    }
}
