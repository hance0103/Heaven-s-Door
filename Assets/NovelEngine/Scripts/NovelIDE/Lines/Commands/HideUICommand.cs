using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace novel
{
    public class HideUICommand : CommandLine
    {
        private bool isShowUI;
        public HideUICommand(int index, bool isShow) : base(index, DialogoueType.CommandLine)
        {
            isShowUI = isShow;
        }

        public override UniTask Execute()
        {

            if (isShowUI)
            {
                NovelManager.Player.DialoguePanel.SetActive(true);
                return UniTask.CompletedTask;
            }
            else
            {
                NovelManager.Player.DialoguePanel.SetActive(false);
                return UniTask.CompletedTask;
            }
        }
    }

}
