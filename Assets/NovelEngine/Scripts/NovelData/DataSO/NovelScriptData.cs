using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [CreateAssetMenu(fileName = "NovelScriptData", menuName = "Novel/NovelScriptData", order = 0)]
    public class NovelScriptData : ScriptableObject
    {
        [SerializeField] private NovelEngine.Scripts.SerializableDict<string, TextAsset> scriptList = new();

        public TextAsset GetScriptByTitle(string title)
        {
            if (scriptList.TryGetValue(title, out var script))
            {
                return script;
            }
            Debug.LogError($"스크립트 {title} 을(를) 찾을 수 없음.");
            return null;
        }
    }
}