using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [CreateAssetMenu(fileName = "NovelVariableData", menuName = "Novel/NovelVariableData", order = 0)]
    public class NovelVariableData : ScriptableObject
    {
        [SerializeField] private NovelEngine.Scripts.SerializableDict<string, NovelVariable> novelVariableDict = new();

        
    }
}
