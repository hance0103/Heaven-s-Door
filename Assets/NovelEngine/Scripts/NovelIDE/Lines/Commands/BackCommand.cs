using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static NovelParser;


namespace novel
{
    [System.Serializable]
    public class BackCommand : CommandLine
    {
        public string backName;
        public string transition;
        public Vector2? pos;
        public float? scale;
        public float? time;


        enum Transition
        {
            Fadein,
            Fadeout,
            SlideRight,
            SlideLeft,
            SlideUp,
            SlideDown,
            None
        }

        private Transition MatchTransition(string str)
        {
            foreach (Transition value in Enum.GetValues(typeof(Transition)))
            {
                if (string.Equals(value.ToString(), str, StringComparison.CurrentCultureIgnoreCase))
                {
                    return value;
                }
            }
            return Transition.None;
        }
        public BackCommand(
            int index, 
            string backName, 
            string transition,              
            Vector2? pos, 
            float? scale, 
            float? time, 
            IfParameter ifParameter = null)
            : base(index, DialogoueType.CommandLine)
        {
            this.backName = backName;
            this.transition = transition;
            this.pos = pos;
            this.scale = scale;
            this.time = time;
            this.ifParameter = ifParameter;
        }

        public override async UniTask Execute()
        {
            // TODO: IF문 처리
            //if ()
            //    return;

            var player = NovelManager.Player;
            var token = player.CommandToken;
            // 기존에 있던 배경 오브젝트 제거


            // 배경 프리팹 불러오기
            GameObject backgroundPrefab = null;
            try
            {
                var handle = Addressables.InstantiateAsync("BackgroundBase",
                            player.BackgroundPanel.transform);
                backgroundPrefab = await handle.Task;
                if (backgroundPrefab == null)
                {
                    Debug.LogError("배경화면 프리팹 인스턴스화 실패");
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            // 데이터에서 배경화면 불러오기
            Texture2D backgroundTexture = NovelManager.Data.background.GetTexture2DByName(backName);
            if (backgroundTexture == null)
            {
                Debug.LogError($"{backName} 배경화면 불러오기 실패");
                return;
            }
            Sprite sprite = Sprite.Create(backgroundTexture,
                                          new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
                                          new Vector2(0.5f, 0.5f));


            if (sprite == null)
            {
                Debug.LogError("스프라이트 Create 실패 " + backName);
                return;
            }

            Image image = backgroundPrefab.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;

                // 위치 조정
                Vector2 percentPos = pos ?? new Vector2(50, 50);
                Vector2 normalizedAnchor = percentPos / 100f;

                RectTransform rectTransform = backgroundPrefab.GetComponent<RectTransform>();

                rectTransform.anchorMin = normalizedAnchor;
                rectTransform.anchorMax = normalizedAnchor;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = Vector2.zero;

                // 크기 조정
                
                //나중에 화면 크기 필요할수도 있을거 같아서 일단 가져옴

                //Vector2 canvasSize = new Vector2(Screen.width, Screen.height);

                if (scale.HasValue)
                    backgroundPrefab.transform.localScale = Vector3.one * scale.Value;

            }


            // 전환효과가 있을 경우
            if (!string.IsNullOrEmpty(transition))
            {
                // TODO
                // 나중에 스위치문으로 바꿀것
                Transition trans = MatchTransition(transition);

                if (trans == Transition.Fadein)
                {
                    if (!backgroundPrefab.TryGetComponent<CanvasGroup>(out var cg))
                        cg = backgroundPrefab.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    float fadeTime = time ?? 0f;
                    if (time > 0f)
                        await NovelUtils.Fade(backgroundPrefab, fadeTime, true, token);
                }
                else
                {
                    // Silde 연출

                    image.type = Image.Type.Filled;
                    image.fillAmount = 0f;
                    switch (trans)
                    {
                        case Transition.SlideRight:
                            {
                                image.fillMethod = Image.FillMethod.Horizontal;
                                image.fillOrigin = (int)Image.OriginHorizontal.Right;
                            }
                            break;
                        case Transition.SlideLeft:
                            {
                                image.fillMethod =  Image.FillMethod.Horizontal;
                                image.fillOrigin = (int)Image.OriginHorizontal.Left;

                            }
                            break;
                        case Transition.SlideUp:
                            {
                                image.fillMethod =  Image.FillMethod.Vertical;
                                image.fillOrigin = (int)Image.OriginVertical.Top;
                            }
                            break;
                        case Transition.SlideDown:
                            {
                                image.fillMethod =  Image.FillMethod.Vertical;
                                image.fillOrigin = (int)Image.OriginVertical.Bottom;
                            }
                            break;
                        case Transition.None:
                            Debug.LogError("해당하는 트랜지션이 없음");
                            break;
                    }
                    float slideTime = time ?? 0f;
                    if (time > 0f)
                        await NovelUtils.Slide(image, slideTime, token);
                    
                }
                

            }

            // 연출이 끝난 후 기존 배경화면 삭제
            if (player.CurrentBackgroundObject != null)
            {
                var beforeBackground = player.CurrentBackgroundObject;
                beforeBackground.SetActive(false);
                Addressables.ReleaseInstance(beforeBackground);
            }

            // 새로 생성한 배경화면을 현재 배경화면으로 설정
            NovelManager.Player.SetBackground(backgroundPrefab);
        }
    }
}