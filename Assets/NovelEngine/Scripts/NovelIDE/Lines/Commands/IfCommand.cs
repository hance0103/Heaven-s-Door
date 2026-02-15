using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NovelParser;

namespace novel
{
    public enum IfType
    {
        If,
        Else,
        None
    }

    [System.Serializable]
    public class IfCommand : CommandLine
    {
        public IfType typeOfIf;
        public string entity;
        public string var;
        public CompOp op;
        public string value;
        

        public IfCommand(
            int index, 
            IfType typeOfIf = IfType.None, 
            string var = null,       
            CompOp op = CompOp.None, 
            string value = null, 
            IfParameter ifParameter = null) 
            : base(index, DialogoueType.CommandLine)
        {
            this.typeOfIf = typeOfIf;
            this.value = value;
            this.var = var;
            this.op = op;
            this.ifParameter = ifParameter;
        }
        private void FindRealValue(string strValue)
        {
            //NovelManager.
        }
        // 변수끼리 비교하는 기능 추가할수도 있음
        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;

            var player = NovelManager.Player;
            
            // 데이터에서 변수값 받아오기
            float realVar = 10; // 임시 데이터

            float realValue = 0;
            
            
            bool returnBool = NovelUtils.ConditinalStateMent(realVar, op, realValue);


            if (returnBool)
            {
                Debug.Log(subLine);
                // 조건이 참이면 서브라인 실행
                if (subLine != null)
                {
                    Debug.Log($"If 조건 참, 서브라인 실행 : {var} {op} {value}");
                    player.SetSublinePlaying(subLine);
                }
                else
                {
                    Debug.LogWarning("IfCommand에 서브라인이 없습니다.");
                }
            }
            else
            {
                Debug.Log($"If 조건 거짓, 서브라인 건너뜀 : {var} {op} {value}");
            }
        }
    }
}
