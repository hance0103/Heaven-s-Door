using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public abstract class NovelLine
    {
        public int index;
        public DialogoueType type;
        protected NovelLine(int index, DialogoueType type)
        {
            this.index = index;
            this.type = type;
        }
    }
}