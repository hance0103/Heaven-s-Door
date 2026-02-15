using NovelEngine.Scripts;
using UnityEngine;

namespace novel
{
    [CreateAssetMenu(fileName = "NovelCharacterData", menuName = "Novel/NovelCharacterData", order = 0)]
    public class NovelCharacterData : ScriptableObject
    {
        [SerializeField] private SerializableDict<string, NovelCharacterSO> charDict = new();
        public NovelCharacterSO GetCharacterByName(string name)
        {
            if (charDict.TryGetValue(name, out var character))
            {
                return character;
            }
            Debug.LogError($"캐릭터 {name} 을(를) 찾을 수 없음.");
            return null;
        }
    }
}