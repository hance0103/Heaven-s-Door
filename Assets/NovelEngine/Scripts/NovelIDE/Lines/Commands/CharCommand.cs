using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static NovelParser;

namespace novel
{
    [System.Serializable]
    public class CharCommand : CommandLine
    {
        private const int BODY_CHILD_INDEX = 0;
        private const int HEAD_CHILD_INDEX = 1;

        string charName;
        string appearance;
        string transition;
        Vector2? pos;
        float? scale;
        float? time;
        private bool isFlip;
        
        CharCommandType charCommandType;
        public CharCommand(
            int index,
            string name,
            string appearance,

            // 이거 나중에 이넘으로 바꿀까
            string transition,
            Vector2? pos,
            float? scale,
            float? time,
            bool isFlip,
            CharCommandType charCommandType = CharCommandType.Show,
            IfParameter ifParameter = null
        ) : base(index, DialogoueType.CommandLine)
        {
            this.charName = name;
            this.appearance = appearance;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.charCommandType = charCommandType;
            this.ifParameter = ifParameter;
            this.isFlip = isFlip;
        }
        public override async UniTask Execute()
        {
            //if (!ifParameter)
            //    return;
            var player = NovelManager.Player;
            var dict = player.currentCharacterDict;
            var token = player.CommandToken;

            try
            {
                if (charCommandType == CharCommandType.HideAll)
                {
                    Debug.Log("Hide All 커맨드 실행");
                    var toRelease = new List<GameObject>(dict.Values).ToArray();
                    foreach (var charObject in toRelease)
                    {
                        if (charObject == null) continue;
                        charObject.SetActive(false);
                        Addressables.ReleaseInstance(charObject);
                    }
                    dict.Clear();
                    return;
                }

                NovelCharacterSO charSO = NovelManager.Data.character.GetCharacterByName(charName);
                // SO가 널이면 캐릭터 불러오기 실패
                if (!charSO)
                {
                    Debug.LogError($"{charName} 캐릭터 불러오기 실패");
                    return;
                }
                switch (charCommandType)
                {
                    case CharCommandType.Show:
                        if (dict.TryGetValue(charSO, out GameObject standingObject) && standingObject != null)
                        {
                            var headObject = standingObject.transform.GetChild(HEAD_CHILD_INDEX).gameObject;

                            if (string.IsNullOrEmpty(appearance))
                            {
                                headObject.SetActive(false);
                            }
                            else
                            {
                                var headSprite = charSO.GetHead(appearance.ToLower());
                                if (headSprite == null)
                                {
                                    Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                                    headObject.SetActive(false);
                                    break;
                                }

                                headObject.SetActive(true);

                                if (!headObject.TryGetComponent<Image>(out var headImage))
                                    headImage = headObject.AddComponent<Image>();


                                headImage.sprite = headSprite;
                                headImage.SetNativeSize();
                                var headTransform = headObject.GetComponent<RectTransform>();
                                headTransform.localPosition = charSO.headOffset;
                                headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);
                            }
                            
                            // 좌우 반전
                            if (isFlip)
                            {
                                standingObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                            }
                        }
                        else  // 현재 캐릭터가 띄워져 있지 않은 경우
                        {
                            GameObject newStanding = null;
                            try
                            {
                                var handle = Addressables.InstantiateAsync("CharacterStandingBase",
                                            player.StandingPanel.transform);
                                newStanding = await handle.Task;
                                if (newStanding == null)
                                {
                                    Debug.LogError("스탠딩 프리팹 인스턴스화 실패");
                                    break;
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                                break;
                            }

                            if (isFlip)
                            {
                                newStanding.transform.rotation = Quaternion.Euler(0, 180, 0);
                            }
                            
                            var bodyObject = newStanding.transform.GetChild(BODY_CHILD_INDEX).gameObject;
                            var headObject = newStanding.transform.GetChild(HEAD_CHILD_INDEX).gameObject;

                            var bodySprite = charSO.body;
                            if (bodySprite == null)
                            {
                                bodyObject.SetActive(false);
                                // Texture2D transparentTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                                // transparentTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
                                // transparentTex.Apply();
                                //
                                // bodySprite = Sprite.Create(
                                //     transparentTex, 
                                //     new Rect(0, 0, 1, 1), 
                                //     new Vector2(0.5f, 0.5f)
                                // );
                            }
                            else
                            {
                                if (!bodyObject.TryGetComponent<Image>(out var bodyImage))
                                    bodyImage = bodyObject.AddComponent<Image>();
                                bodyImage.sprite = bodySprite;
                                bodyImage.SetNativeSize();
                                var bodyTransform = bodyObject.GetComponent<RectTransform>();
                                bodyTransform.sizeDelta = new Vector2(bodySprite.rect.width, bodySprite.rect.height);
                            }

                            if (string.IsNullOrEmpty(appearance))
                            {
                                headObject.SetActive(false);
                            }
                            else
                            {
                                var headSprite = charSO.GetHead(appearance.ToLower());
                                if (headSprite == null)
                                {
                                    Debug.LogError($"{charName}의 {appearance} 표정 존재하지 않음");
                                    headObject.SetActive(false);
                                }
                                else
                                {
                                    headObject.SetActive(true);
                                    if (!headObject.TryGetComponent<Image>(out var headImage))
                                        headImage = headObject.AddComponent<Image>();
                                    headImage.sprite = headSprite;
                                    headImage.SetNativeSize();
                                    var headTransform = headObject.GetComponent<RectTransform>();
                                    headTransform.localPosition = charSO.headOffset;
                                    headTransform.sizeDelta = new Vector2(headSprite.rect.width, headSprite.rect.height);
                                }
                            }

                            // 위치/스케일
                            {
                                var rt = newStanding.GetComponent<RectTransform>();
                                Vector2 percentPos = pos ?? new Vector2(50, 50);
                                Vector2 anchor = percentPos / 100f;

                                rt.anchorMin = anchor;
                                rt.anchorMax = anchor;
                                rt.pivot = new Vector2(0.5f, 0.5f);
                                rt.anchoredPosition = Vector2.zero;

                                if (scale.HasValue)
                                {
                                    rt.localScale = Vector3.one * scale.Value;
                                }
                            }


                            bool doFadeIn = string.Equals(transition, "fadein");


                            newStanding.SetActive(true);

                            // 페이드 인 연출
                            if (doFadeIn)
                            {
                                if (!newStanding.TryGetComponent<CanvasGroup>(out var cg))
                                    cg = newStanding.AddComponent<CanvasGroup>();

                                cg.alpha = doFadeIn ? 0f : 1f;  // fadein일 경우 0으로 시작

                                float time = this.time ?? 0f;
                                if (time > 0f)
                                    await NovelUtils.Fade(newStanding, time, true, token);
                                else
                                    cg.alpha = 1f; // 즉시 엔드
                            }

                            dict[charSO] = newStanding;
              
                        }
                        
                        break;
                    case CharCommandType.Hide:
                        //Debug.Log($"Hide Character : {charName}");
                        if (!dict.TryGetValue(charSO, out GameObject hideObject) || hideObject == null)
                        {
                            // 이미 없으면 조용히 정리
                            dict.Remove(charSO);
                            break;
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(transition))
                            {
                                float dur = this.time ?? 0f;
                                if (dur > 0f)
                                    await NovelUtils.Fade(hideObject, dur, false, token);
                                else
                                {
                                    // 즉시 엔드
                                    if (!hideObject.TryGetComponent<CanvasGroup>(out var cg))
                                        cg = hideObject.AddComponent<CanvasGroup>();
                                    cg.alpha = 0f;
                                }
                            }
                        }
                        finally
                        {
                            hideObject.SetActive(false);
                            Addressables.ReleaseInstance(hideObject);
                            dict.Remove(charSO);
                        }
                        break;
                    case CharCommandType.Effect:
                        // 아직 고려 x
                        break;
                    default:
                        Debug.LogError("할당되지 않은 Character 커맨드");
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

        }
    }
}

