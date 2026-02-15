using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class GotoCommand : CommandLine
    {
        public string script;
        public string label;
        public GotoCommand(
            int index,
            string script,
            string label, 
            IfParameter ifParameter = null) 
            : base(index, DialogoueType.CommandLine)
        {
            this.label = label;
            this.script = script;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            //Debug.Log($"GOTO 실행 \nScript : {script}\nLabel : {label}");
            
            //if (!ifParameter)
            //    return;
           // Debug.Log($"goto 실행 : {label}");
           if (string.IsNullOrEmpty(script))
           {
               NovelManager.Player.CurrentAct.JumpToLabel(label);
           }
           else
           {
               if (string.IsNullOrEmpty(label))
               {
                   // 다른 스크립트 처음부터 플레이
               }
               else
               {
                   // 다른 스크립트의 특정 라벨부터 플레이
               }
           }
           
            
        }
    }
}
