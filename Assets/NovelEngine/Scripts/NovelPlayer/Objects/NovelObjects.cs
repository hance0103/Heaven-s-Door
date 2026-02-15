using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace novel
{
    public class NovelObjects : MonoBehaviour
    {
        [SerializeField]
        private NovelObjectType type;
        public NovelObjectType GetNovelObjectType()
        {
            return type;
        }

    }
    public enum NovelObjectType
    {
        Blur,
        BackgroundPanel,
        StandingPanel,
        DialogPanel,
        NovelText,
        NamePanel,
        NameText,
        NextButton,
        ChoicePanel
    }
}
