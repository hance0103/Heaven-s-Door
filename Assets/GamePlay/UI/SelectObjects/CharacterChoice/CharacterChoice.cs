using GamePlay;
using GamePlay.UI.SelectObjects;
using UnityEngine;

public class CharacterChoice : SelectObject
{
    [SerializeField] private SystemEnum.Character character;
    
    public override void Execute()
    {
        // 해당하는 캐릭터 대화
        GameManager.Instance.StartNovelScene("Karen_beforeText");
    }
}
