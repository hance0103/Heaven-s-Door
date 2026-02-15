using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace novel
{
    [System.Serializable]
    public class NovelAct
    {
        [SerializeReference]
        public List<NovelLine> novelLines;
        [SerializeField]
        private int currentIndex = 0;
        public int CurrentIndex => currentIndex;
        public NovelLine GetNextLine()
        {
            if (currentIndex >= novelLines.Count) return null;

            return novelLines[currentIndex++];
        }

        public NovelLine GetLineFromIndex(int index)
        {
            return novelLines.FirstOrDefault(line => line.index == index);
        }
        public void JumpToLabel(string label)
        {
            int targetIndex = NovelManager.Player.LabelDict.GetValue(label);
            
            
            //Debug.Log($"{label}라벨의 인덱스 : {targetIndex}");
            currentIndex = targetIndex - 1;
        }

        public void JumpToIndex(int index)
        {
            currentIndex = index - 1;
        }
        public void ResetAct() => currentIndex = 0;


        public int GetIndex()
        {
            return currentIndex;
        }
    }
}
