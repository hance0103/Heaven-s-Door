using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class ChoiceCommand : CommandLine
    {
        public string argument;

        public ChoiceCommand(
            int index,
            string argument,
            IfParameter ifParameter = null) 
            : base(index, DialogoueType.CommandLine)
        {
            this.argument = argument;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            var player = NovelManager.Player;
            var token = player.CommandToken;
            
            // 선택지 프리팹 불러오기
            GameObject choicePrefab = null;
            try
            {
                var handle = Addressables.InstantiateAsync("ChoiceButtonBase",
                            player.ChoicePanel.transform);
                choicePrefab = await handle.Task;
                if (choicePrefab == null)
                {
                    Debug.LogError("선택지 프리팹 인스턴스화 실패");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return;
            }

            choicePrefab.GetComponentInChildren<TextMeshProUGUI>().text = argument;
            Button choiceButton = choicePrefab.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => OnClickChoiceButton());

            player.currentChoices.Add(this, choicePrefab);

            // if 패러미터가 설정되어 있으면 조건 검사
            if (!(ifParameter.Op == CompOp.None))
            {
                // if 패러미터가 false이면 버튼 비활성화
                // 임시 데이터 10 들어감 이거 데이터에서 변수값 받아오도록 수정해야함
                float rignt;
                float left;


                if (NovelUtils.ConditinalStateMent(10, ifParameter.Op, 10))   
                {
                    //Debug.Log("선택지 활성화");
                    choiceButton.interactable = true;
                }
                else
                {
                    //Debug.Log("선택지 비활성화");
                    choiceButton.interactable = false;
                }
            }
            //.Log($"선택지 프리팹 생성 : {argument}");
            player.DialoguePanel.SetActive(false);
            player.isChoiceOn = true;
        }
        private void OnClickChoiceButton()
        {
            //Debug.Log($"선택지 {argument} 클릭");
            NovelPlayer player = NovelManager.Player;
            if (subLine == null)
            {
                Debug.LogError("선택지에 연결된 서브라인이 없습니다.");
            }
            else
            {
                player.SetSublinePlaying(subLine);
            }
                

            // 선택지 오브젝트 제거
            foreach (Transform child in player.ChoicePanel.transform)
            {
                child.gameObject.SetActive(false);
                Addressables.ReleaseInstance(child.gameObject);
            }


            player.currentChoices.Clear();
            player.Resume();
            player.isChoiceOn = false;
        }
    }

}

