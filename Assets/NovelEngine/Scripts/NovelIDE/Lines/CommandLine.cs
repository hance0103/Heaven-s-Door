using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public abstract class CommandLine : NovelLine
    {
        [SerializeReference]
        public NovelLine subLine;
        public IfParameter ifParameter;
        public CommandLine(int index, DialogoueType type) : base(index, type) { }

        public abstract UniTask Execute();
    }
}