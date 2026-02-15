using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class LabelLine : NovelLine
    {
        public string labelName;

        public LabelLine(int index, string labelName) : base(index, DialogoueType.LabelLine)
        {
            this.labelName = labelName;
        }
    }
}

