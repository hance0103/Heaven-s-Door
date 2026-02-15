using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class SetCommand : CommandLine
    {
        private NovelVariable left;
        private NovelVariable right;
        private CalcOp op;
        
        public SetCommand(int index) : base(index, DialogoueType.CommandLine)
        {
            
        }

        public override UniTask Execute()
        {
            if (right.parameter is IntParameter)
            {
                switch (op)
                {
                    case CalcOp.Assign:
                        break;
                    case CalcOp.Increase:
                        break;
                    case CalcOp.Decrease:
                        break;
                    case CalcOp.IncreaseAmount:
                        break;
                    case CalcOp.DecreaseAmount:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // int 패러미터가 아니면 할당 연산자만 가능 나중에 float도 추가할것
                
                
            }
            

            return UniTask.CompletedTask;
        }
    }

}
