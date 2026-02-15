using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NormalLine : NovelLine
    {
        
        public string line;

        public NormalLine(int index, string line) : base(index, DialogoueType.NormalLine)
        {
            this.line = line;
        }
    }
}