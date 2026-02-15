
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [CreateAssetMenu(fileName = "NovelBackgroundData", menuName = "Novel/NovelBackgroundData", order = 0)]
    public class NovelBackgroundData : ScriptableObject
    {
        [SerializeField] private NovelEngine.Scripts.SerializableDict<string, Texture2D> novelBackgroundDict = new();
        public Texture2D GetTexture2DByName(string name)
        {
            novelBackgroundDict.TryGetValue(name, out var texture);
            return texture;
        }
    }
}
